using ROIO.Utils;
using System.Collections.Generic;

public partial class ZC {
    [PacketHandler(HEADER, "ZC_HUNTING_QUEST_INFO")]
    public class HUNTING_QUEST_INFO : InPacket {
        public const PacketHeader HEADER = PacketHeader.ZC_HUNTING_QUEST_INFO;
        public PacketHeader Header => HEADER;

        public class Progress {
            public int QuestId;
            public int MobId;
            public ushort Total;
            public ushort Current;
        }

        public List<Progress> Updates = new List<Progress>();

        public void Read(MemoryStreamReader br, int size) {
            while (br.Position + 12 <= br.Length) {
                Updates.Add(new Progress {
                    QuestId = br.ReadInt(),
                    MobId = br.ReadInt(),
                    Total = br.ReadUShort(),
                    Current = br.ReadUShort()
                });
            }
        }
    }
}
