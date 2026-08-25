using ROIO.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/**
 * Note:
 * When working with packets, sometimes the server will send multiple
 * packets all at once. In those cases, we'll receive them all together.
 * That's why we iterate in a while once we receive bytes to read
 */
public class PacketSerializer {

    public struct PacketInfo {
        public int Size;
        public Type Type;
    }

    public MemoryStream Memory { get; set; }
    public int BytesToSkip { get; set; }

    public static Dictionary<ushort, PacketInfo> RegisteredPackets;

    private IPacketHandler PacketHandler;

    static PacketSerializer() {
        RegisteredPackets = new Dictionary<ushort, PacketInfo>();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(type => type.GetInterface("InPacket") != null)) {
            object[] attributes = type.GetCustomAttributes(typeof(PacketHandlerAttribute), true); // get the attributes of the packet.
            if (attributes.Length == 0)
                continue;
            PacketHandlerAttribute ma = (PacketHandlerAttribute) attributes[0];
            RegisteredPackets.Add(ma.MethodId, new PacketInfo { Size = ma.Size, Type = type });
        }
    }

    public PacketSerializer(IPacketHandler packetHandler) {
        PacketHandler = packetHandler;
        Memory = new MemoryStream();
    }

    public void Reset() {
        Memory?.Dispose();
        Memory = new MemoryStream();
        BytesToSkip = 0;
    }

    public void EnqueueBytes(byte[] data, int size) {
        int pos = (int) Memory.Position;
        Memory.Position = Memory.Length;
        Memory.Write(data, 0, size);
        Memory.Position = pos;

        ReadPacket();
    }

    private void ReadPacket() {
        if (BytesToSkip > 0) {
            long remaining = Memory.Length - Memory.Position;
            int skipped = Math.Min(BytesToSkip, (int) remaining);
            Memory.Position += skipped;
            BytesToSkip -= skipped;
        }

        while (Memory.Length - Memory.Position >= 2) {
            long packetStart = Memory.Position;

            // Commands are always the first two bytes. Variable-size packets then
            // carry a two-byte total packet length.
            var tmp = new byte[2];
            Memory.Read(tmp, 0, 2);
            ushort cmd = BitConverter.ToUInt16(tmp, 0);

            if (!RegisteredPackets.TryGetValue(cmd, out var packetInfo)) {
                // We cannot safely skip an unknown command because its size is unknown.
                Debug.LogWarning($"Received Unknown Command: {string.Format("0x{0:x4}", cmd)}\nProbably: {(PacketHeader) cmd}");
                DumpReceivedPacket(cmd, -1, Memory.Length - Memory.Position);
                Memory.Position = packetStart;
                break;
            }

            int size = packetInfo.Size;
            bool isFixed = size > 0;

            if (!isFixed) {
                // The TCP stream may have split the two-byte length field itself.
                if (Memory.Length - Memory.Position < 2) {
                    Memory.Position = packetStart;
                    break;
                }

                Memory.Read(tmp, 0, 2);
                size = BitConverter.ToUInt16(tmp, 0);
            }

            int headerSize = isFixed ? 2 : 4;
            int payloadSize = size - headerSize;

            if (payloadSize < 0) {
                Debug.LogWarning($"Received invalid packet size {size} for {(PacketHeader) cmd}");
                Memory.Position = packetStart;
                break;
            }

            // TCP is a byte stream. A logical packet can arrive over multiple reads,
            // so only deserialize after every payload byte is available.
            if (Memory.Length - Memory.Position < payloadSize) {
                Memory.Position = packetStart;
                break;
            }

            byte[] data = new byte[payloadSize];
            Memory.Read(data, 0, payloadSize);

            ConstructorInfo ci = packetInfo.Type.GetConstructor(Type.EmptyTypes);
            if (ci == null) {
                Debug.LogWarning($"Packet {(PacketHeader) cmd} has no parameterless constructor");
                continue;
            }

            InPacket packet = (InPacket) ci.Invoke(null);
            using var br = new MemoryStreamReader(data);
            packet.Read(br, payloadSize);

            ThreadManager.ExecuteOnMainThread(() => {
                PacketHandler.OnPacketReceived(packet);
            });

            PacketReceived?.Invoke(cmd, size, packet);
            DumpReceivedPacket(cmd, size, Memory.Length - Memory.Position);
        }

        if (Memory.Length - Memory.Position > 0) {
            MemoryStream ms = new MemoryStream();
            ms.Write(Memory.GetBuffer(), (int) Memory.Position, (int) (Memory.Length - Memory.Position));
            Memory.Dispose();
            Memory = ms;
        } else if (Memory.Length > 0) {
            // Drop fully consumed bytes so the receive buffer cannot grow forever.
            Memory.Dispose();
            Memory = new MemoryStream();
        }
    }

    private static void DumpReceivedPacket(ushort cmd, int size, long remainingSize, InPacket packet = null) {
#if DUMP_RECEIVED_PACKET
        try {
            var log = $"{string.Format("0x{0:x3}", cmd)} \tReceived Size:{size} \tRegistered Size:{RegisteredPackets.Where(it => it.Key == cmd).FirstOrDefault().Value.Size} \tRemaining Size: {remainingSize} \t// {(PacketHeader) cmd}";
            Debug.Log(log);
        } catch (Exception e) {
            Debug.LogException(e);
        }
#endif
    }

    public event Action<ushort, int, InPacket> PacketReceived;
    public delegate void OnPacketReceived(ushort cmd, int size, InPacket packet);
}