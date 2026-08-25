using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Network {
    [TestFixture]
    public class NetworkClientTests {
        private GameObject NetworkObject;
        private NetworkClient Client;
        private MethodInfo TryHandleReceivedPacket;

        [SetUp]
        public void SetUp() {
            NetworkObject = new GameObject("NetworkClientTests");
            Client = NetworkObject.AddComponent<NetworkClient>();
            Client.Start();
            TryHandleReceivedPacket = typeof(NetworkClient).GetMethod("TryHandleReceivedPacket", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(TryHandleReceivedPacket, Is.Not.Null);
        }

        [TearDown]
        public void TearDown() {
            if (NetworkObject != null) {
                Object.DestroyImmediate(NetworkObject);
            }
        }

        [Test]
        public void HookPacket_AllowsMultipleIndependentSubscribers() {
            int firstCalls = 0;
            int secondCalls = 0;

            PacketSerializer.OnPacketReceived first = (cmd, size, packet) => firstCalls++;
            PacketSerializer.OnPacketReceived second = (cmd, size, packet) => secondCalls++;

            Client.HookPacket(ZC.NOTIFY_TIME.HEADER, first);
            Client.HookPacket(ZC.NOTIFY_TIME.HEADER, second);
            Dispatch(new ZC.NOTIFY_TIME());

            Assert.That(firstCalls, Is.EqualTo(1));
            Assert.That(secondCalls, Is.EqualTo(1));
        }

        [Test]
        public void UnhookPacket_RemovesOnlyRequestedSubscriber() {
            int firstCalls = 0;
            int secondCalls = 0;

            PacketSerializer.OnPacketReceived first = (cmd, size, packet) => firstCalls++;
            PacketSerializer.OnPacketReceived second = (cmd, size, packet) => secondCalls++;

            Client.HookPacket(ZC.NOTIFY_TIME.HEADER, first);
            Client.HookPacket(ZC.NOTIFY_TIME.HEADER, second);
            Assert.That(Client.UnhookPacket(ZC.NOTIFY_TIME.HEADER, first), Is.True);

            Dispatch(new ZC.NOTIFY_TIME());

            Assert.That(firstCalls, Is.EqualTo(0));
            Assert.That(secondCalls, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateHook_IsIgnored() {
            int calls = 0;
            PacketSerializer.OnPacketReceived hook = (cmd, size, packet) => calls++;

            Client.HookPacket(ZC.NOTIFY_TIME.HEADER, hook);
            Client.HookPacket(ZC.NOTIFY_TIME.HEADER, hook);
            Dispatch(new ZC.NOTIFY_TIME());

            Assert.That(calls, Is.EqualTo(1));
        }

        private void Dispatch(InPacket packet) {
            Client.OnPacketReceived(packet);
            TryHandleReceivedPacket.Invoke(Client, null);
        }
    }
}
