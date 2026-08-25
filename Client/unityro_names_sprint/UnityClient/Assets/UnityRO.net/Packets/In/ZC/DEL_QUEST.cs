using ROIO.Utils;

public partial class ZC {
    [PacketHandler(HEADER, "ZC_DEL_QUEST", SIZE)]
    public class DEL_QUEST : InPacket {
        public const PacketHeader HEADER = PacketHeader.ZC_DEL_QUEST;
        public const int SIZE = 6;
        public PacketHeader Header => HEADER;
        public int QuestId;
        public void Read(MemoryStreamReader br, int size) { QuestId = br.ReadInt(); }
    }
}
