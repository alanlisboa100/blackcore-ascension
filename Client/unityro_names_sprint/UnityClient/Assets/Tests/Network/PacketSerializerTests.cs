using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Tests.Network {
    [TestFixture]
    public class PacketSerializerTests {
        private sealed class RecordingPacketHandler : IPacketHandler {
            public readonly List<InPacket> Packets = new List<InPacket>();

            public void OnPacketReceived(InPacket packet) {
                Packets.Add(packet);
            }
        }

        [SetUp]
        public void SetUp() {
            // Flush any work left by another test before asserting local counts.
            ThreadManager.UpdateMain();
        }

        [Test]
        public void FixedSizePacket_WaitsForFullPayload_WhenTcpFragmentsIt() {
            var handler = new RecordingPacketHandler();
            var serializer = new PacketSerializer(handler);
            byte[] packet = BuildFixedMsgPacket(0x1234);

            serializer.EnqueueBytes(packet.Take(3).ToArray(), 3);
            ThreadManager.UpdateMain();
            Assert.That(handler.Packets, Is.Empty);

            byte[] tail = packet.Skip(3).ToArray();
            serializer.EnqueueBytes(tail, tail.Length);
            ThreadManager.UpdateMain();

            Assert.That(handler.Packets.Count, Is.EqualTo(1));
            Assert.That(handler.Packets[0], Is.TypeOf<ZC.MSG>());
            Assert.That(((ZC.MSG) handler.Packets[0]).MessageID, Is.EqualTo(0x1234));
        }

        [Test]
        public void VariableSizePacket_WaitsForCompleteLengthAndPayload_WhenTcpFragmentsIt() {
            var handler = new RecordingPacketHandler();
            var serializer = new PacketSerializer(handler);
            byte[] packet = BuildVariablePlayerChatPacket("hello mobile");

            // Deliberately split inside the two-byte variable-length field.
            serializer.EnqueueBytes(packet.Take(3).ToArray(), 3);
            ThreadManager.UpdateMain();
            Assert.That(handler.Packets, Is.Empty);

            byte[] tail = packet.Skip(3).ToArray();
            serializer.EnqueueBytes(tail, tail.Length);
            ThreadManager.UpdateMain();

            Assert.That(handler.Packets.Count, Is.EqualTo(1));
            Assert.That(handler.Packets[0], Is.TypeOf<ZC.NOTIFY_PLAYERCHAT>());
            Assert.That(((ZC.NOTIFY_PLAYERCHAT) handler.Packets[0]).Message, Is.EqualTo("hello mobile"));
        }

        [Test]
        public void SpriteChange2_UsesRathenaElevenByteLayout_WithoutConsumingNextPacket() {
            var handler = new RecordingPacketHandler();
            var serializer = new PacketSerializer(handler);

            byte[] spriteChange = BuildSpriteChange2Packet(42, ZC.SPRITE_CHANGE2.LookType.LOOK_WEAPON, 1101, 2101);
            byte[] nextPacket = BuildFixedMsgPacket(77);
            byte[] combined = spriteChange.Concat(nextPacket).ToArray();

            serializer.EnqueueBytes(combined, combined.Length);
            ThreadManager.UpdateMain();

            Assert.That(ZC.SPRITE_CHANGE2.SIZE, Is.EqualTo(11));
            Assert.That(handler.Packets.Count, Is.EqualTo(2));
            Assert.That(handler.Packets[0], Is.TypeOf<ZC.SPRITE_CHANGE2>());
            Assert.That(handler.Packets[1], Is.TypeOf<ZC.MSG>());

            var look = (ZC.SPRITE_CHANGE2) handler.Packets[0];
            Assert.That(look.GID, Is.EqualTo(42));
            Assert.That(look.type, Is.EqualTo(ZC.SPRITE_CHANGE2.LookType.LOOK_WEAPON));
            Assert.That(look.value, Is.EqualTo(1101));
            Assert.That(look.value2, Is.EqualTo(2101));
            Assert.That(((ZC.MSG) handler.Packets[1]).MessageID, Is.EqualTo(77));
        }

        [Test]
        public void MultiplePacketsInOneTcpRead_AreAllDispatched() {
            var handler = new RecordingPacketHandler();
            var serializer = new PacketSerializer(handler);
            byte[] first = BuildFixedMsgPacket(1);
            byte[] second = BuildFixedMsgPacket(2);
            byte[] combined = first.Concat(second).ToArray();

            serializer.EnqueueBytes(combined, combined.Length);
            ThreadManager.UpdateMain();

            Assert.That(handler.Packets.Count, Is.EqualTo(2));
            Assert.That(((ZC.MSG) handler.Packets[0]).MessageID, Is.EqualTo(1));
            Assert.That(((ZC.MSG) handler.Packets[1]).MessageID, Is.EqualTo(2));
        }


        [Test]
        public void VariableOutPacket_RemainsVariable_WhenSameInstanceIsSerializedAgain() {
            var packet = new TestVariableOutPacket();

            using var firstStream = new MemoryStream();
            packet.Write((byte) 0xAA);
            packet.Send(firstStream);

            using var secondStream = new MemoryStream();
            packet.Write((byte) 0xBB);
            packet.Send(secondStream);

            byte[] first = firstStream.ToArray();
            byte[] second = secondStream.ToArray();

            Assert.That(first.Length, Is.EqualTo(5));
            Assert.That(second.Length, Is.EqualTo(5));
            Assert.That(BitConverter.ToUInt16(first, 2), Is.EqualTo(5));
            Assert.That(BitConverter.ToUInt16(second, 2), Is.EqualTo(5));
            Assert.That(first[4], Is.EqualTo(0xAA));
            Assert.That(second[4], Is.EqualTo(0xBB));
        }

        [Test]
        public void FixedOutPacket_SerializesHeaderAndPayloadWithoutVariableLengthField() {
            var packet = new TestFixedOutPacket();
            using var stream = new MemoryStream();

            packet.Write((uint) 0x12345678);
            packet.Send(stream);
            byte[] bytes = stream.ToArray();

            Assert.That(bytes.Length, Is.EqualTo(6));
            Assert.That(BitConverter.ToUInt16(bytes, 0), Is.EqualTo((ushort) ZC.MSG.HEADER));
            Assert.That(BitConverter.ToUInt32(bytes, 2), Is.EqualTo(0x12345678));
        }

        private sealed class TestVariableOutPacket : OutPacket {
            public TestVariableOutPacket() : base(ZC.NOTIFY_PLAYERCHAT.HEADER, PacketHandlerAttribute.VariableSize) { }
        }

        private sealed class TestFixedOutPacket : OutPacket {
            public TestFixedOutPacket() : base(ZC.MSG.HEADER, 6) { }
        }

        private static byte[] BuildSpriteChange2Packet(uint gid, ZC.SPRITE_CHANGE2.LookType type, short value, short value2) {
            return BitConverter.GetBytes((ushort) ZC.SPRITE_CHANGE2.HEADER)
                .Concat(BitConverter.GetBytes(gid))
                .Concat(new[] { (byte) type })
                .Concat(BitConverter.GetBytes(value))
                .Concat(BitConverter.GetBytes(value2))
                .ToArray();
        }

        private static byte[] BuildFixedMsgPacket(ushort messageId) {
            return BitConverter.GetBytes((ushort) ZC.MSG.HEADER)
                .Concat(BitConverter.GetBytes(messageId))
                .ToArray();
        }

        private static byte[] BuildVariablePlayerChatPacket(string message) {
            byte[] payload = Encoding.ASCII.GetBytes(message);
            ushort totalSize = (ushort) (4 + payload.Length);

            return BitConverter.GetBytes((ushort) ZC.NOTIFY_PLAYERCHAT.HEADER)
                .Concat(BitConverter.GetBytes(totalSize))
                .Concat(payload)
                .ToArray();
        }
    }
}
