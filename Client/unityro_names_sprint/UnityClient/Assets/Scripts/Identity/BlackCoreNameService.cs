using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Black Core: Ascension display-name layer.
///
/// IMPORTANT: This class never mutates the server-side identity of an entity.
/// Player names remain exactly what the account/character server sends. NPC and
/// monster aliases are presentation-only so rAthena scripts, GIDs, packet data,
/// quests and combat logic keep using the original values.
/// </summary>
public static class BlackCoreNameService {
    private static readonly string[] BrazilianPlayerNames = {
        "Caio", "Davi", "Ravi", "Theo", "Gael", "Miguel", "Arthur", "Heitor",
        "Lucas", "Pedro", "Gabriel", "Matheus", "Rafael", "Bruno", "Diego", "Vitor",
        "Felipe", "Guilherme", "Henrique", "Yuri", "Luan", "Levi", "Nicolas", "Joao",
        "Luna", "Maya", "Jade", "Alice", "Laura", "Helena", "Sofia", "Manuela",
        "Beatriz", "Julia", "Vitoria", "Isabela", "Clara", "Luisa", "Marina", "Leticia",
        "Cecilia", "Aurora", "Valentina", "Bianca", "Livia", "Melissa", "Nina", "Lara"
    };

    private static readonly string[] MmoSuffixes = {
        "Brasa", "Nox", "Lume", "Rune", "Vale", "Nova", "Fera", "Vox",
        "Core", "Onyx", "Lobo", "Runa", "Astra", "Sol", "Nyx", "Vento",
        "Raio", "Umbra", "Fenix", "Draco", "Arc", "Zero", "Nexus", "Eclipse"
    };

    private static readonly string[] NpcFirstNames = {
        "Ana", "Bia", "Bruna", "Caio", "Clara", "Davi", "Diego", "Felipe",
        "Gabriel", "Gabi", "Helena", "Iara", "Jade", "Joao", "Julia", "Leo",
        "Livia", "Lucas", "Luna", "Malu", "Marcos", "Marina", "Maya", "Nina",
        "Paulo", "Pedro", "Rafa", "Ravi", "Renan", "Sofia", "Theo", "Vini", "Yasmin", "Yuri"
    };

    // Presentation-only aliases for recognizable starter/common creatures.
    // Names intentionally lean generic/original instead of exposing RO branding.
    private static readonly Dictionary<string, string> MonsterAliases = new Dictionary<string, string> {
        { "poring", "Geleia Rosa" },
        { "drops", "Gota Azul" },
        { "poporing", "Lodo Verde" },
        { "marin", "Gota do Mar" },
        { "magmaring", "Gota de Magma" },
        { "lunatic", "Coelho Lunar" },
        { "fabre", "Lagartinha Verde" },
        { "chonchon", "Mosca do Campo" },
        { "steelchonchon", "Mosca de Aço" },
        { "rocker", "Grilo do Vale" },
        { "picky", "Pintinho Bravo" },
        { "willow", "Salgueiro Vivo" },
        { "elderwillow", "Salgueiro Ancião" },
        { "spore", "Cogumelo Bravo" },
        { "poisonspore", "Cogumelo Tóxico" },
        { "mandragora", "Mandragora" },
        { "wolf", "Lobo Cinzento" },
        { "desertwolf", "Lobo do Sertão" },
        { "smokie", "Guaxinim da Mata" },
        { "hornet", "Vespa Dourada" },
        { "andre", "Formiga Operaria" },
        { "deniro", "Formiga Rubra" },
        { "piere", "Formiga Verde" },
        { "thiefbug", "Barata das Ruinas" },
        { "thiefbugmale", "Barata Cascuda" },
        { "thiefbugfemale", "Barata Rainha" },
        { "tarou", "Rato do Esgoto" },
        { "familiar", "Morcego da Caverna" },
        { "skeleton", "Esqueleto Errante" },
        { "soldierskeleton", "Soldado Ossudo" },
        { "archerskeleton", "Arqueiro Ossudo" },
        { "zombie", "Morto-Vivo" },
        { "ghoul", "Devorador" },
        { "orcwarrior", "Guerreiro Orc" },
        { "orclady", "Matriarca Orc" },
        { "orchero", "Campeao Orc" },
        { "goblin", "Goblin do Vale" },
        { "kobold", "Kobold Selvagem" },
        { "condor", "Condor" },
        { "pecopeco", "Ave do Sertao" },
        { "muka", "Cacto Bravo" },
        { "golem", "Golem de Pedra" },
        { "metaller", "Besouro Metálico" },
        { "argiope", "Aranha Venenosa" },
        { "argios", "Aranha do Deserto" },
        { "sidewinder", "Serpente Listrada" },
        { "anacondaq", "Anaconda" },
        { "boa", "Jiboia" },
        { "yoyo", "Macaquinho" },
        { "savaged", "Javali Jovem" },
        { "savage", "Javali Selvagem" },
        { "baphomet", "Senhor do Abismo" },
        { "baphometjr", "Herdeiro do Abismo" },
        { "mistress", "Rainha Vespa" },
        { "moonlightflower", "Raposa Lunar" },
        { "osiris", "Faraó Sombrio" },
        { "doppelganger", "O Reflexo" },
        { "drake", "Capitao Espectral" },
        { "eddga", "Tigre Ancestral" },
        { "phreeoni", "Olho da Areia" },
        { "maya", "Rainha Formiga" },
        { "stormyknight", "Cavaleiro da Geada" },
        { "darklord", "Lorde das Sombras" }
    };

    private static readonly string[] CreatureRoots = {
        "Fera", "Caçador", "Rastejante", "Guardião", "Devorador", "Espinho",
        "Predador", "Errante", "Sentinela", "Mordedor", "Bruto", "Vigia"
    };

    private static readonly string[] CreatureOrigins = {
        "do Vale", "de Ônix", "da Mata", "das Ruínas", "do Eclipse", "da Bruma",
        "da Lua", "do Sertão", "da Geada", "da Forja", "do Abismo", "do Núcleo"
    };

    private static readonly Dictionary<string, string> GenericNpcRoles = new Dictionary<string, string> {
        { "kafra", "Atendente" },
        { "employee", "Atendente" },
        { "tooldealer", "Mercador" },
        { "itemdealer", "Mercador" },
        { "weapondealer", "Armeiro" },
        { "armordealer", "Armadureiro" },
        { "blacksmith", "Ferreiro" },
        { "healer", "Curandeiro" },
        { "guide", "Guia" },
        { "stylist", "Estilista" },
        { "inn", "Hospedeiro" },
        { "merchant", "Mercador" },
        { "guard", "Guarda" },
        { "soldier", "Guarda" },
        { "trainer", "Instrutor" }
    };

    public static string ResolveEntityDisplayName(Entity entity, string rawName) {
        if (entity == null) {
            return Beautify(rawName);
        }

        switch (entity.Type) {
            case EntityType.PC:
                return ResolvePlayerName(rawName, entity.GID);
            case EntityType.MOB:
            case EntityType.DISGUISED:
                return ResolveMonsterName(rawName);
            case EntityType.NPC:
                return ResolveNpcName(rawName, entity.GID);
            case EntityType.PET:
            case EntityType.HOM:
            case EntityType.MERC:
            case EntityType.ELEM:
                return ResolveCompanionName(rawName, entity.GID);
            default:
                return Beautify(rawName);
        }
    }

    /// <summary>
    /// Player names are never replaced if the server sent a real one.
    /// Only empty/prototype names receive a deterministic fallback.
    /// </summary>
    public static string ResolvePlayerName(string rawName, uint gid) {
        if (!IsGenericOrEmpty(rawName, "player", "character", "unknown")) {
            return rawName.Trim();
        }

        return BuildSuggestedPlayerName(gid == 0 ? 1u : gid);
    }

    public static string ResolveMonsterName(string rawName) {
        if (string.IsNullOrWhiteSpace(rawName)) return "Criatura do Núcleo";

        // The Black Core rAthena overrides already send curated Portuguese names.
        // Preserve them instead of feeding them back into the legacy-name fallback.
        if (MonsterAliases.Values.Any(value => string.Equals(value, rawName.Trim(), StringComparison.OrdinalIgnoreCase))) {
            return rawName.Trim();
        }

        var key = NormalizeKey(rawName);
        if (MonsterAliases.TryGetValue(key, out var alias)) {
            return alias;
        }

        return GenerateCreatureAlias(rawName);
    }

    public static string ResolveNpcName(string rawName, uint gid) {
        var key = NormalizeKey(rawName);

        foreach (var role in GenericNpcRoles) {
            if (key.Contains(role.Key)) {
                return $"{NpcFirstNames[StableIndex(gid, NpcFirstNames.Length)]} — {role.Value}";
            }
        }

        // Named legacy NPCs also receive a stable Black Core identity.
        // The raw server name remains untouched for scripts and packet logic.
        return $"{NpcFirstNames[StableIndex(gid == 0 ? StableHash(rawName) : gid, NpcFirstNames.Length)]} — Morador";
    }

    public static string ResolveCompanionName(string rawName, uint gid) {
        if (!IsGenericOrEmpty(rawName, "pet", "homunculus", "mercenary", "elemental", "unknown")) {
            var monsterAlias = ResolveMonsterName(rawName);
            return monsterAlias;
        }

        var petNames = new[] { "Pingo", "Tico", "Nina", "Juca", "Bolota", "Fubá", "Pipoca", "Zeca", "Lupi", "Mel" };
        return petNames[StableIndex(gid, petNames.Length)];
    }

    public static string SuggestPlayerName() {
        unchecked {
            var seed = (uint)(DateTime.UtcNow.Ticks ^ Environment.TickCount);
            return BuildSuggestedPlayerName(seed);
        }
    }

    public static string BuildSuggestedPlayerName(uint seed) {
        var first = BrazilianPlayerNames[StableIndex(seed, BrazilianPlayerNames.Length)];
        var suffixSeed = Fnv1A(seed, 0xB1ACC0DEu);
        var suffix = MmoSuffixes[StableIndex(suffixSeed, MmoSuffixes.Length)];
        return first + suffix;
    }

    public static bool ShouldKeepNameVisible(Entity entity) {
        if (entity == null) return false;

        switch (entity.Type) {
            case EntityType.PC:
            case EntityType.NPC:
            case EntityType.MOB:
            case EntityType.DISGUISED:
            case EntityType.PET:
            case EntityType.HOM:
            case EntityType.MERC:
            case EntityType.ELEM:
                return true;
            default:
                return false;
        }
    }

    public static Color GetNameColor(Entity entity) {
        if (entity == null) return Color.white;

        if (entity.HasAuthority) {
            return new Color(0.38f, 0.95f, 1f, 1f);
        }

        switch (entity.Type) {
            case EntityType.PC:
                return new Color(0.92f, 0.96f, 1f, 1f);
            case EntityType.NPC:
                return new Color(1f, 0.82f, 0.38f, 1f);
            case EntityType.MOB:
            case EntityType.DISGUISED:
                return new Color(1f, 0.53f, 0.48f, 1f);
            case EntityType.PET:
            case EntityType.HOM:
            case EntityType.MERC:
            case EntityType.ELEM:
                return new Color(0.52f, 1f, 0.66f, 1f);
            default:
                return Color.white;
        }
    }

    public static Color GetNameOutlineColor(Entity entity) {
        if (entity != null && entity.HasAuthority) {
            return new Color(0.02f, 0.18f, 0.22f, 0.9f);
        }

        return new Color(0.02f, 0.025f, 0.04f, 0.92f);
    }

    private static string GenerateCreatureAlias(string rawName) {
        var seed = StableHash(rawName);
        var root = CreatureRoots[StableIndex(seed, CreatureRoots.Length)];
        var originSeed = Fnv1A(seed, 0xC0DEFACEu);
        var origin = CreatureOrigins[StableIndex(originSeed, CreatureOrigins.Length)];
        return $"{root} {origin}";
    }

    private static uint StableHash(string value) {
        unchecked {
            uint hash = 2166136261;
            foreach (var c in value ?? string.Empty) {
                hash ^= c;
                hash *= 16777619;
            }
            return hash;
        }
    }

    private static bool IsGenericOrEmpty(string value, params string[] genericKeys) {
        if (string.IsNullOrWhiteSpace(value)) return true;
        var key = NormalizeKey(value);
        return genericKeys.Any(generic => key == NormalizeKey(generic));
    }

    private static int StableIndex(uint seed, int length) {
        if (length <= 0) return 0;
        return (int)(seed % (uint)length);
    }

    private static uint Fnv1A(uint a, uint b) {
        unchecked {
            uint hash = 2166136261;
            hash = (hash ^ (a & 0xff)) * 16777619;
            hash = (hash ^ ((a >> 8) & 0xff)) * 16777619;
            hash = (hash ^ ((a >> 16) & 0xff)) * 16777619;
            hash = (hash ^ ((a >> 24) & 0xff)) * 16777619;
            hash = (hash ^ (b & 0xff)) * 16777619;
            hash = (hash ^ ((b >> 8) & 0xff)) * 16777619;
            hash = (hash ^ ((b >> 16) & 0xff)) * 16777619;
            hash = (hash ^ ((b >> 24) & 0xff)) * 16777619;
            return hash;
        }
    }

    private static string NormalizeKey(string value) {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized) {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    private static string Beautify(string value) {
        if (string.IsNullOrWhiteSpace(value)) return "Sem Nome";

        var cleaned = value.Replace('_', ' ').Trim();
        if (cleaned.Length == 0) return "Sem Nome";

        // Keep intentional mixed-case player/NPC names intact. Upper snake/caps data
        // gets humanized for a cleaner in-world label.
        if (cleaned.Any(char.IsLower)) return cleaned;

        return CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(cleaned.ToLowerInvariant());
    }
}
