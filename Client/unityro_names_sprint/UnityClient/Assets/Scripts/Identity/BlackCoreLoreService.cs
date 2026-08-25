using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Presentation-layer identity for Black Core: Ascension.
///
/// Server IDs, map resource names, skill IDs, item IDs and packet values remain untouched.
/// Only player-facing text is transformed. This lets the client acquire its own identity
/// without breaking rAthena compatibility while the server-side content migration is staged.
/// </summary>
public static class BlackCoreLoreService {
    public const string CurrencyName = "Núcleos";

    private static readonly Dictionary<string, string> MapAliases = new Dictionary<string, string> {
        { "prontera", "Nova Aurora" },
        { "prt", "Nova Aurora" },
        { "izlude", "Porto Prisma" },
        { "iz", "Porto Prisma" },
        { "geffen", "Torre Nox" },
        { "gef", "Torre Nox" },
        { "payon", "Vale Cedro" },
        { "pay", "Vale Cedro" },
        { "morocc", "Dunas de Ônix" },
        { "moc", "Dunas de Ônix" },
        { "alberta", "Porto Brasa" },
        { "alb", "Porto Brasa" },
        { "aldebaran", "Cronovale" },
        { "yuno", "Altavila" },
        { "juno", "Altavila" },
        { "lighthalzen", "Neo Lumen" },
        { "einbroch", "Forja Rubra" },
        { "einbech", "Vila da Forja" },
        { "comodo", "Costa Lunar" },
        { "cmd", "Costa Lunar" },
        { "umbala", "Raiz Antiga" },
        { "niflheim", "Véu Sombrio" },
        { "gonryun", "Jardim Celeste" },
        { "louyang", "Vale de Jade" },
        { "amatsu", "Ilha do Sol" },
        { "ayothaya", "Santuário Verde" },
        { "brasilis", "Bravamar" },
        { "veins", "Cânion Rubro" },
        { "rachel", "Auríria" },
        { "hugel", "Campo Alto" },
        { "moscovia", "Bosque Boreal" },
        { "mosk_dun", "Cripta Boreal" },
        { "glast_heim", "Fortaleza Umbra" },
        { "prt_sewb", "Esgotos de Nova Aurora" },
        { "moc_pryd", "Pirâmide de Ônix" },
        { "gef_dun", "Catacumbas Nox" },
        { "pay_dun", "Santuário de Cedro" },
        { "orcsdun", "Covil dos Orcs" }
    };

    private static readonly Dictionary<string, string> JobAliases = new Dictionary<string, string> {
        { "novice", "Iniciado" },
        { "swordman", "Guerreiro" },
        { "swordsman", "Guerreiro" },
        { "mage", "Arcanista" },
        { "archer", "Arqueiro" },
        { "acolyte", "Devoto" },
        { "merchant", "Mercador" },
        { "thief", "Ladino" },
        { "knight", "Cavaleiro" },
        { "wizard", "Mago de Guerra" },
        { "hunter", "Caçador" },
        { "priest", "Sacerdote" },
        { "blacksmith", "Mestre Ferreiro" },
        { "assassin", "Assassino" },
        { "crusader", "Templário" },
        { "sage", "Sábio Arcano" },
        { "bard", "Bardo" },
        { "dancer", "Dançarina" },
        { "monk", "Monge" },
        { "alchemist", "Alquimista" },
        { "rogue", "Renegado" },
        { "super novice", "Prodígio" },
        { "gunslinger", "Pistoleiro" },
        { "ninja", "Shinobi" },
        { "lord knight", "Lorde da Lâmina" },
        { "high wizard", "Arquimago" },
        { "sniper", "Atirador de Elite" },
        { "high priest", "Sumo Sacerdote" },
        { "whitesmith", "Forjador Supremo" },
        { "assassin cross", "Executor Sombrio" },
        { "paladin", "Paladino" },
        { "professor", "Mestre Arcano" },
        { "clown", "Menestrel" },
        { "gypsy", "Musa" },
        { "champion", "Campeão" },
        { "creator", "Criador" },
        { "stalker", "Predador" },
        { "rune knight", "Cavaleiro Rúnico" },
        { "warlock", "Bruxo Arcano" },
        { "ranger", "Guardião da Mata" },
        { "arch bishop", "Arcebispo" },
        { "mechanic", "Engenheiro de Guerra" },
        { "guillotine cross", "Ceifador Sombrio" },
        { "royal guard", "Guardião Real" },
        { "sorcerer", "Feiticeiro" },
        { "minstrel", "Menestrel" },
        { "wanderer", "Andarilha" },
        { "sura", "Punho Celestial" },
        { "genetic", "Bioalquimista" },
        { "shadow chaser", "Caçador de Sombras" },
        { "star emperor", "Imperador Astral" },
        { "soul reaper", "Ceifador de Almas" },
        { "rebellion", "Insurgente" },
        { "kagerou", "Kage" },
        { "oboro", "Oboro" },
        { "summoner", "Invocador" }
    };

    private static readonly Dictionary<string, string> SkillAliases = new Dictionary<string, string> {
        { "bash", "Golpe Brutal" },
        { "magnum break", "Explosão Rubra" },
        { "provoke", "Desafio" },
        { "endure", "Pele de Ferro" },
        { "increase hp recovery", "Fôlego de Aço" },
        { "sword mastery", "Domínio da Lâmina" },
        { "two-handed sword mastery", "Domínio da Montante" },
        { "bowling bash", "Corte Tempestuoso" },
        { "brandish spear", "Investida Imperial" },
        { "aura blade", "Lâmina de Aura" },
        { "fire bolt", "Faísca Rubra" },
        { "cold bolt", "Estilhaço Gélido" },
        { "lightning bolt", "Raio Celeste" },
        { "fire ball", "Orbe de Brasas" },
        { "fire wall", "Muralha Ígnea" },
        { "frost diver", "Lança de Gelo" },
        { "thunderstorm", "Tempestade Elétrica" },
        { "soul strike", "Impacto Astral" },
        { "safety wall", "Barreira Arcana" },
        { "stone curse", "Olhar de Pedra" },
        { "napalm beat", "Pulso Astral" },
        { "double strafe", "Disparo Duplo" },
        { "arrow shower", "Chuva de Flechas" },
        { "owl's eye", "Olhar do Falcão" },
        { "vulture's eye", "Mira Longa" },
        { "improve concentration", "Foco do Caçador" },
        { "blitz beat", "Ataque do Falcão" },
        { "falcon assault", "Investida Aérea" },
        { "sharp shooting", "Tiro Perfurante" },
        { "heal", "Cura Radiante" },
        { "blessing", "Bênção" },
        { "increase agility", "Passo Celeste" },
        { "angelus", "Guarda Sagrada" },
        { "pneuma", "Véu de Vento" },
        { "holy light", "Luz Sagrada" },
        { "resurrection", "Retorno da Alma" },
        { "sanctuary", "Santuário" },
        { "double attack", "Golpe Duplo" },
        { "envenom", "Lâmina Venenosa" },
        { "hiding", "Passo Sombrio" },
        { "steal", "Mão Leve" },
        { "back slide", "Recuo Sombrio" },
        { "sonic blow", "Rajada Sônica" },
        { "cloaking", "Véu Sombrio" },
        { "grimtooth", "Presa Sombria" },
        { "mammonite", "Golpe Dourado" },
        { "discount", "Barganha" },
        { "overcharge", "Venda Esperta" },
        { "cart revolution", "Impacto de Carga" },
        { "vending", "Loja de Rua" },
        { "teleport", "Salto de Núcleo" },
        { "warp portal", "Portal do Núcleo" }
    };

    private static readonly Dictionary<string, string> ItemAliases = new Dictionary<string, string> {
        { "red potion", "Poção Rubra" },
        { "orange potion", "Poção Solar" },
        { "yellow potion", "Poção Dourada" },
        { "white potion", "Poção de Luz" },
        { "blue potion", "Essência Azul" },
        { "green potion", "Antídoto Verde" },
        { "red herb", "Erva Rubra" },
        { "yellow herb", "Erva Dourada" },
        { "white herb", "Erva de Luz" },
        { "blue herb", "Erva Azul" },
        { "green herb", "Erva Verde" },
        { "apple", "Maçã do Vale" },
        { "banana", "Banana Dourada" },
        { "carrot", "Cenoura Selvagem" },
        { "meat", "Carne Temperada" },
        { "jellopy", "Fragmento Gelatinoso" },
        { "fluff", "Tufo Macio" },
        { "clover", "Trevo do Vale" },
        { "feather", "Pluma Leve" },
        { "iron", "Ferro Bruto" },
        { "steel", "Aço Temperado" },
        { "phracon", "Minério Bruto" },
        { "emveretarcon", "Liga Arcana" },
        { "fly wing", "Asa de Salto" },
        { "butterfly wing", "Asa de Retorno" },
        { "knife", "Faca de Campo" },
        { "main gauche", "Punhal do Vale" },
        { "sword", "Espada de Ferro" },
        { "falchion", "Lâmina Curva" },
        { "blade", "Lâmina de Guerra" },
        { "katana", "Katana Rubra" },
        { "bow", "Arco de Caça" },
        { "cross bow", "Besta de Caça" },
        { "crossbow", "Besta de Caça" },
        { "rod", "Cajado Arcano" },
        { "staff", "Cajado de Guerra" },
        { "mace", "Maça de Ferro" },
        { "club", "Clava Reforçada" },
        { "axe", "Machado de Ferro" },
        { "spear", "Lança de Guarda" },
        { "cotton shirt", "Camisa de Viajante" },
        { "jacket", "Jaqueta de Couro" },
        { "guard", "Escudo de Guarda" },
        { "buckler", "Broquel de Ferro" },
        { "sandals", "Sandálias de Viagem" },
        { "shoes", "Botas de Viagem" }
    };

    private static readonly Dictionary<string, string> DialogueAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
        { "Prontera", "Nova Aurora" },
        { "Izlude", "Porto Prisma" },
        { "Geffen", "Torre Nox" },
        { "Payon", "Vale Cedro" },
        { "Morroc", "Dunas de Ônix" },
        { "Morocc", "Dunas de Ônix" },
        { "Alberta", "Porto Brasa" },
        { "Al De Baran", "Cronovale" },
        { "Aldebaran", "Cronovale" },
        { "Juno", "Altavila" },
        { "Yuno", "Altavila" },
        { "Lighthalzen", "Neo Lumen" },
        { "Einbroch", "Forja Rubra" },
        { "Comodo", "Costa Lunar" },
        { "Umbala", "Raiz Antiga" },
        { "Niflheim", "Véu Sombrio" },
        { "Rune-Midgard", "Núcleo Central" },
        { "Rune Midgard", "Núcleo Central" },
        { "Midgard", "Núcleo Central" },
        { "Adventurer", "Viajante" },
        { "Adventurers", "Viajantes" },
        { "Kafra", "Guia do Núcleo" },
        { "Emperium", "Núcleo Imperial" },
        { "MVP", "Arquichefe" },
        { "Ragnarok", "Black Core" },
        { "rAthena", "Núcleo do Servidor" },
        { "UnityRO", "Black Core" },
        { "Zeny", CurrencyName },
    };

    public static string ResolveMapName(string rawMapName) {
        if (string.IsNullOrWhiteSpace(rawMapName)) return "Região Desconhecida";

        var file = Path.GetFileNameWithoutExtension(rawMapName).ToLowerInvariant();
        if (MapAliases.TryGetValue(file, out var exact)) return exact;

        var prefix = Regex.Replace(file, @"(_fild|_field|_dun|_dungeon|_in|_inside)\d*$", string.Empty);
        if (MapAliases.TryGetValue(prefix, out var root)) {
            var numberMatch = Regex.Match(file, @"(\d+)$");
            var suffix = numberMatch.Success ? $" {ToRoman(ParseInt(numberMatch.Value))}" : string.Empty;
            if (file.Contains("dun")) return $"Ruínas de {root}{suffix}";
            if (file.Contains("fild") || file.Contains("field")) return $"Campos de {root}{suffix}";
            return root + suffix;
        }

        return BeautifyIdentifier(file);
    }

    public static string ResolveJobName(string rawName, int jobId, int sex) {
        var normalized = Normalize(rawName)
            .Replace("baby ", string.Empty)
            .Replace("transcendent ", string.Empty)
            .Replace("high ", "high ");

        if (JobAliases.TryGetValue(normalized, out var alias)) return alias;

        // Common suffix/prefix variants from different client tables.
        foreach (var pair in JobAliases) {
            if (normalized.Contains(pair.Key)) return pair.Value;
        }

        return BeautifyIdentifier(rawName);
    }

    public static string ResolveSkillName(string rawName, int skillId = 0) {
        var normalized = Normalize(rawName);
        if (SkillAliases.TryGetValue(normalized, out var alias)) return alias;
        return BeautifyIdentifier(rawName);
    }

    public static string ResolveItemName(string rawName, int itemId = 0) {
        var normalized = Normalize(rawName);
        if (ItemAliases.TryGetValue(normalized, out var alias)) return alias;

        // Avoid touching special/custom names we do not understand yet.
        // This keeps content stable while the curated catalog grows.
        return BeautifyIdentifier(rawName);
    }

    public static string ResolveQuestTitle(int questId, string rawTitle) {
        if (string.IsNullOrWhiteSpace(rawTitle)) return $"Missão {questId}";
        return ApplyDialogueIdentity(rawTitle);
    }

    public static string ApplyDialogueIdentity(string text) {
        if (string.IsNullOrEmpty(text)) return text;

        var result = text;
        foreach (var pair in DialogueAliases) {
            result = Regex.Replace(result, $@"\b{Regex.Escape(pair.Key)}\b", pair.Value, RegexOptions.IgnoreCase);
        }

        return result;
    }

    private static string Normalize(string value) {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var s = value.Trim().Trim('"').Replace('_', ' ').Replace('-', ' ');
        s = Regex.Replace(s, @"\s+", " ");
        return RemoveDiacritics(s).ToLowerInvariant();
    }

    private static string BeautifyIdentifier(string value) {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var cleaned = value.Trim().Trim('"').Replace('_', ' ').Replace('-', ' ');
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        return CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(cleaned.ToLowerInvariant());
    }

    private static string RemoveDiacritics(string text) {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized) {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static int ParseInt(string value) => int.TryParse(value, out var n) ? n : 0;

    private static string ToRoman(int number) {
        switch (number) {
            case 1: return "I";
            case 2: return "II";
            case 3: return "III";
            case 4: return "IV";
            case 5: return "V";
            case 6: return "VI";
            case 7: return "VII";
            case 8: return "VIII";
            case 9: return "IX";
            case 10: return "X";
            default: return number > 0 ? number.ToString() : string.Empty;
        }
    }
}
