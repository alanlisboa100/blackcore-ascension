using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public abstract class OutPacket : NetworkPacket {

    public PacketHeader Header { get; private set; }
    public int Size;

    private readonly bool IsFixed;
    private readonly List<byte> PayloadBuffer = new List<byte>(128);

    public OutPacket(PacketHeader header, int size) {
        Header = header;
        Size = size;
        IsFixed = size > 0;
    }

    public virtual void Send() {
        NetworkClient.SendPacket(this);
    }

    public void Send(Stream stream) {
        if (stream == null || !stream.CanWrite) {
            throw new IOException($"Cannot send {Header}: network stream is not writable");
        }

        int headerSize = IsFixed ? 2 : 4;
        int packetSize = PayloadBuffer.Count + headerSize;

        if (!IsFixed) {
            Size = packetSize;
        } else if (Size != packetSize) {
            Debug.LogWarning($"Packet {Header} declared size {Size} but serialized {packetSize} bytes");
        }

        byte[] packet = new byte[packetSize];
        byte[] headerBytes = BitConverter.GetBytes((ushort) Header);
        System.Buffer.BlockCopy(headerBytes, 0, packet, 0, headerBytes.Length);

        int payloadOffset = 2;
        if (!IsFixed) {
            byte[] sizeBytes = BitConverter.GetBytes((ushort) packetSize);
            System.Buffer.BlockCopy(sizeBytes, 0, packet, 2, sizeBytes.Length);
            payloadOffset = 4;
        }

        if (PayloadBuffer.Count > 0) {
            PayloadBuffer.CopyTo(0, packet, payloadOffset, PayloadBuffer.Count);
        }

        stream.Write(packet, 0, packet.Length);
        stream.Flush();
        PayloadBuffer.Clear();
    }

    public void Write(int value) => PayloadBuffer.AddRange(BitConverter.GetBytes(value));
    public void Write(long value) => PayloadBuffer.AddRange(BitConverter.GetBytes(value));
    public void Write(byte value) => PayloadBuffer.Add(value);
    public void Write(short value) => PayloadBuffer.AddRange(BitConverter.GetBytes(value));
    public void Write(ushort value) => PayloadBuffer.AddRange(BitConverter.GetBytes(value));
    public void Write(ulong value) => PayloadBuffer.AddRange(BitConverter.GetBytes(value));
    public void Write(uint value) => PayloadBuffer.AddRange(BitConverter.GetBytes(value));
    public void Write(string value) => PayloadBuffer.AddRange(Encoding.ASCII.GetBytes(value));

    public void Write(string value, int size) {
        for (int i = 0; i < size; i++) {
            PayloadBuffer.Add(i < value.Length ? (byte) value[i] : (byte) 0);
        }
    }

    public void WritePos(short x, short y, byte dir) {
        Write((byte) (x >> 2));
        Write((byte) ((x << 6) | ((y >> 4) & 0x3f)));
        Write((byte) ((y << 4) | (dir & 0xf)));
    }
}
