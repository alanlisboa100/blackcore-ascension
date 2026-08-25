using NUnit.Framework;
using ROIO.Utils;
using System.IO;
using System.Text;

public class BlackCoreQuestPacketTests {
    [Test]
    public void QuestList3_ParsesRealRathenaObjectiveProgress() {
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms)) {
            bw.Write(1);             // quest count
            bw.Write(90001);         // quest id
            bw.Write((byte)0);       // active
            bw.Write((uint)0);       // start
            bw.Write((uint)0);       // expire
            bw.Write((ushort)1);     // objectives
            bw.Write(90001000);      // hunt id
            bw.Write(0);             // mob type
            bw.Write(1002);          // mob id
            bw.Write((ushort)0);     // min level
            bw.Write((ushort)0);     // max level
            bw.Write((ushort)4);     // killed
            bw.Write((ushort)10);    // total
            var name = new byte[24];
            Encoding.ASCII.GetBytes("Poring").CopyTo(name, 0);
            bw.Write(name);

            var packet = new ZC.ALL_QUEST_LIST3();
            packet.Read(new MemoryStreamReader(ms.ToArray()), (int)ms.Length + 4);

            Assert.AreEqual(1, packet.Quests.Count);
            Assert.AreEqual(90001, packet.Quests[0].QuestId);
            Assert.AreEqual(4, packet.Quests[0].Objectives[0].Killed);
            Assert.AreEqual(10, packet.Quests[0].Objectives[0].Total);
            Assert.AreEqual("Poring", packet.Quests[0].Objectives[0].Name);
        }
    }

    [Test]
    public void HuntingQuestInfo_ParsesProgressUpdate() {
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms)) {
            bw.Write(90002);
            bw.Write(1039);
            bw.Write((ushort)1);
            bw.Write((ushort)1);

            var packet = new ZC.HUNTING_QUEST_INFO();
            packet.Read(new MemoryStreamReader(ms.ToArray()), (int)ms.Length + 4);

            Assert.AreEqual(1, packet.Updates.Count);
            Assert.AreEqual(90002, packet.Updates[0].QuestId);
            Assert.AreEqual(1, packet.Updates[0].Current);
        }
    }

    [Test]
    public void AddQuestEx_ParsesTimesAndAllThreeObjectives() {
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms)) {
            bw.Write(90003);          // quest id
            bw.Write((byte)1);        // active
            bw.Write((uint)1234);     // start time
            bw.Write((uint)5678);     // expire time
            bw.Write((ushort)3);      // objectives

            for (int i = 0; i < 3; i++) {
                bw.Write(910000 + i); // hunt id
                bw.Write(0);          // mob type
                bw.Write(1002 + i);   // mob id
                bw.Write((ushort)0);  // min level
                bw.Write((ushort)0);  // max level
                bw.Write((ushort)(5 + i));
                var name = new byte[24];
                Encoding.ASCII.GetBytes("Target" + i).CopyTo(name, 0);
                bw.Write(name);
            }

            var packet = new ZC.ADD_QUEST_EX();
            packet.Read(new MemoryStreamReader(ms.ToArray()), (int)ms.Length);

            Assert.AreEqual((uint)1234, packet.StartTime);
            Assert.AreEqual((uint)5678, packet.ExpireTime);
            Assert.AreEqual(3, packet.Objectives.Count);
            Assert.AreEqual(7, packet.Objectives[2].Total);
        }
    }
}
