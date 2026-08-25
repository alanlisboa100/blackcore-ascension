using ROIO.Utils;
using System.Collections.Generic;

public partial class ZC {
    [PacketHandler(HEADER, "ZC_ALL_QUEST_LIST3")]
    public class ALL_QUEST_LIST3 : InPacket {
        public const PacketHeader HEADER = PacketHeader.ZC_ALL_QUEST_LIST3;
        public PacketHeader Header => HEADER;

        public class Objective {
            public int HuntId;
            public int MobType;
            public int MobId;
            public ushort MinLevel;
            public ushort MaxLevel;
            public ushort Killed;
            public ushort Total;
            public string Name;
        }

        public class QuestEntry {
            public int QuestId;
            public byte State;
            public uint StartTime;
            public uint ExpireTime;
            public List<Objective> Objectives = new List<Objective>();
        }

        public List<QuestEntry> Quests = new List<QuestEntry>();

        public void Read(MemoryStreamReader br, int size) {
            int count = br.ReadInt();
            for (int i = 0; i < count && br.Position < br.Length; i++) {
                var quest = new QuestEntry {
                    QuestId = br.ReadInt(),
                    State = (byte)br.ReadByte(),
                    StartTime = br.ReadUInt(),
                    ExpireTime = br.ReadUInt()
                };
                int objectives = br.ReadUShort();
                for (int j = 0; j < objectives && br.Position + 44 <= br.Length; j++) {
                    quest.Objectives.Add(new Objective {
                        HuntId = br.ReadInt(),
                        MobType = br.ReadInt(),
                        MobId = br.ReadInt(),
                        MinLevel = br.ReadUShort(),
                        MaxLevel = br.ReadUShort(),
                        Killed = br.ReadUShort(),
                        Total = br.ReadUShort(),
                        Name = br.ReadBinaryString(24)
                    });
                }
                Quests.Add(quest);
            }
        }
    }
}
