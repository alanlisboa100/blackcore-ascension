using NUnit.Framework;

public class BlackCoreNameServiceTests {
    [Test]
    public void PlayerName_IsPreserved_WhenServerProvidesOne() {
        Assert.AreEqual("JuninhoBR", BlackCoreNameService.ResolvePlayerName("JuninhoBR", 10));
    }

    [Test]
    public void GenericPlayerName_GetsDeterministicFallback() {
        var first = BlackCoreNameService.ResolvePlayerName("Player", 12345);
        var second = BlackCoreNameService.ResolvePlayerName("Player", 12345);
        Assert.AreEqual(first, second);
        Assert.IsNotEmpty(first);
    }

    [TestCase("Poring", "Geleia Rosa")]
    [TestCase("LUNATIC", "Coelho Lunar")]
    [TestCase("Desert_Wolf", "Lobo do Sertão")]
    [TestCase("Baphomet", "Senhor do Abismo")]
    public void CommonMonsters_UseBlackCoreAliases(string serverName, string expected) {
        Assert.AreEqual(expected, BlackCoreNameService.ResolveMonsterName(serverName));
    }

    [Test]
    public void UnknownLegacyMonster_GetsStableOriginalAlias() {
        var first = BlackCoreNameService.ResolveMonsterName("Legacy_Monster_123");
        var second = BlackCoreNameService.ResolveMonsterName("Legacy_Monster_123");
        Assert.AreEqual(first, second);
        Assert.AreNotEqual("Legacy Monster 123", first);
    }

    [Test]
    public void NamedLegacyNpc_GetsStableBrazilianIdentity() {
        var first = BlackCoreNameService.ResolveNpcName("Old Ragnarok NPC", 777);
        var second = BlackCoreNameService.ResolveNpcName("Old Ragnarok NPC", 777);
        Assert.AreEqual(first, second);
        StringAssert.Contains("Morador", first);
    }

    [Test]
    public void SuggestedName_IsStableForSeed() {
        Assert.AreEqual(
            BlackCoreNameService.BuildSuggestedPlayerName(987654u),
            BlackCoreNameService.BuildSuggestedPlayerName(987654u)
        );
    }
}
