using ROIO.Utils;
using System.Collections.Generic;

public partial class ZC {
    [PacketHandler(HEADER, "ZC_ADD_QUEST_EX", SIZE)]
    public class ADD_QUEST_EX : InPacket {
        public const PacketHeader HEADER = PacketHeader.ZC_ADD_QUEST_EX;
        public const int SIZE = 143;
        public PacketHeader Header => HEADER;

        public int QuestId;
        public byte State;
        public uint StartTime;
        public uint ExpireTime;
        public List<ALL_QUEST_LIST3.Objective> Objectives = new List<ALL_QUEST_LIST3.Objective>();

        public void Read(MemoryStreamReader br, int size) {
            // rAthena PACKET_ZC_ADD_QUEST_EX (0x09F9):
            // questID.L active.B startTime.L expireTime.L count.W objectives[3].
            QuestId = br.ReadInt();
            State = (byte) br.ReadByte();
            StartTime = br.ReadUInt();
            ExpireTime = br.ReadUInt();
            int count = br.ReadUShort();

            // Each PACKET_ZC_ADD_QUEST_EX objective is 42 bytes for the
            // protocol version used by this client.
            for (int i = 0; i < count && br.Position + 42 <= br.Length; i++) {
                Objectives.Add(new ALL_QUEST_LIST3.Objective {
                    HuntId = br.ReadInt(),
                    MobType = br.ReadInt(),
                    MobId = br.ReadInt(),
                    MinLevel = br.ReadUShort(),
                    MaxLevel = br.ReadUShort(),
                    Killed = 0,
                    Total = br.ReadUShort(),
                    Name = br.ReadBinaryString(24)
                });
            }
        }
    }
}
