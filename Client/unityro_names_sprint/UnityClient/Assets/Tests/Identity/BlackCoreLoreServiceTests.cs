using NUnit.Framework;

public class BlackCoreLoreServiceTests {
    [TestCase("prontera", "Nova Aurora")]
    [TestCase("geffen", "Torre Nox")]
    [TestCase("payon", "Vale Cedro")]
    [TestCase("morocc", "Dunas de Ônix")]
    [TestCase("brasilis", "Bravamar")]
    public void ResolveMapName_UsesBlackCoreRegions(string source, string expected) {
        Assert.AreEqual(expected, BlackCoreLoreService.ResolveMapName(source));
    }

    [Test]
    public void ResolveMapName_FieldPreservesInternalResourceButChangesDisplay() {
        Assert.AreEqual("Campos de Nova Aurora I", BlackCoreLoreService.ResolveMapName("prt_fild01.rsw"));
    }

    [TestCase("Swordman", "Guerreiro")]
    [TestCase("Mage", "Arcanista")]
    [TestCase("Archer", "Arqueiro")]
    [TestCase("Rune Knight", "Cavaleiro Rúnico")]
    [TestCase("Guillotine Cross", "Ceifador Sombrio")]
    public void ResolveJobName_UsesOriginalClassIdentity(string source, string expected) {
        Assert.AreEqual(expected, BlackCoreLoreService.ResolveJobName(source, 0, 0));
    }

    [TestCase("Bash", "Golpe Brutal")]
    [TestCase("Fire Bolt", "Faísca Rubra")]
    [TestCase("Double Strafe", "Disparo Duplo")]
    [TestCase("Heal", "Cura Radiante")]
    public void ResolveSkillName_UsesBlackCoreCombatLanguage(string source, string expected) {
        Assert.AreEqual(expected, BlackCoreLoreService.ResolveSkillName(source));
    }

    [TestCase("Red Potion", "Poção Rubra")]
    [TestCase("Jellopy", "Fragmento Gelatinoso")]
    [TestCase("Bow", "Arco de Caça")]
    public void ResolveItemName_UsesBlackCoreItems(string source, string expected) {
        Assert.AreEqual(expected, BlackCoreLoreService.ResolveItemName(source));
    }

    [Test]
    public void ApplyDialogueIdentity_RebrandsPlacesAndCurrency() {
        var text = BlackCoreLoreService.ApplyDialogueIdentity("Welcome to Prontera. It costs 500 Zeny.");
        Assert.AreEqual("Welcome to Nova Aurora. It costs 500 Núcleos.", text);
    }
}
