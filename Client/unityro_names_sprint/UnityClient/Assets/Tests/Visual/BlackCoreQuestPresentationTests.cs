using NUnit.Framework;

public class BlackCoreQuestPresentationTests {
    [Test]
    public void FormatsLegacyQuestVocabularyAndIdentity() {
        var result = BlackCoreQuestPresentation.Format("Objective: Kill 10 monsters in Prontera. Reward: 500 Zeny.");
        Assert.That(result, Does.Contain("OBJETIVO"));
        Assert.That(result, Does.Contain("RECOMPENSA"));
        Assert.That(result, Does.Contain("Nova Aurora"));
        Assert.That(result, Does.Contain("Núcleos"));
        Assert.That(result, Does.Not.Contain("Prontera"));
        Assert.That(result, Does.Not.Contain("Zeny"));
    }
}
