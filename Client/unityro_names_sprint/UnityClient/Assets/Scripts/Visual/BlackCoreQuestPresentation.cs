using System;
using System.Text.RegularExpressions;

/// <summary>
/// Presentation-only formatter for scripted quest dialogue. Server scripts and
/// quest variables remain authoritative; this only makes common quest language
/// clearer and more Black Core-like in the client.
/// </summary>
public static class BlackCoreQuestPresentation {
    public static string Format(string raw) {
        if (string.IsNullOrWhiteSpace(raw)) return raw;
        var text = BlackCoreLoreService.ApplyDialogueIdentity(raw);

        text = Replace(text, @"\bquest\b", "missão");
        text = Replace(text, @"\bobjective(s)?\b", "objetivo");
        text = Replace(text, @"\breward(s)?\b", "recompensa");
        text = Replace(text, @"\bexperience\b", "experiência");
        text = Replace(text, @"\bbase exp\b", "EXP Base");
        text = Replace(text, @"\bjob exp\b", "EXP de Caminho");
        text = Replace(text, @"\bkill\b", "derrote");
        text = Replace(text, @"\bcollect\b", "colete");
        text = Replace(text, @"\bcompleted\b", "concluída");
        text = Replace(text, @"\bcomplete\b", "concluir");

        text = Regex.Replace(text, @"(?im)^\s*(objetivo|objetivos)\s*:?\s*", "<color=#65E8FF><b>OBJETIVO</b></color> • ");
        text = Regex.Replace(text, @"(?im)^\s*(recompensa|recompensas)\s*:?\s*", "<color=#C797FF><b>RECOMPENSA</b></color> • ");
        text = Regex.Replace(text, @"(?im)^\s*(progresso)\s*:?\s*", "<color=#FFD36A><b>PROGRESSO</b></color> • ");
        return text;
    }

    private static string Replace(string source, string pattern, string replacement) {
        return Regex.Replace(source, pattern, replacement, RegexOptions.IgnoreCase);
    }
}
