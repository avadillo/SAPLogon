using System.ComponentModel;
using System.Reflection;

namespace SAPTools.Ticket.Extensions;

public enum SAPLanguage {
    /// <summary>
    /// Serbian (SR)
    /// </summary>
    [Description("Serbian (SR)")] SR = '0',
    /// <summary>
    /// Chinese (ZH)
    /// </summary>
    [Description("Chinese (ZH)")] ZH = '1',
    /// <summary>
    /// Thai (TH)
    /// </summary>
    [Description("Thai (TH)")] TH = '2',
    /// <summary>
    /// Korean (KO)
    /// </summary>
    [Description("Korean (KO)")] KO = '3',
    /// <summary>
    /// Romanian (RO)
    /// </summary>
    [Description("Romanian (RO)")] RO = '4',
    /// <summary>
    /// Slovenian (SL)
    /// </summary>
    [Description("Slovenian (SL)")] SL = '5',
    /// <summary>
    /// Croatian (HR)
    /// </summary>
    [Description("Croatian (HR)")] HR = '6',
    /// <summary>
    /// Malay (MS)
    /// </summary>
    [Description("Malay (MS)")] MS = '7',
    /// <summary>
    /// Ukrainian (UK)
    /// </summary>
    [Description("Ukrainian (UK)")] UK = '8',
    /// <summary>
    /// Estonian (ET)
    /// </summary>
    [Description("Estonian (ET)")] ET = '9',
    /// <summary>
    /// Arabic (AR)
    /// </summary>
    [Description("Arabic (AR)")] AR = 'A',
    /// <summary>
    /// Hebrew (HE)
    /// </summary>
    [Description("Hebrew (HE)")] HE = 'B',
    /// <summary>
    /// Czech (CS)
    /// </summary>
    [Description("Czech (CS)")] CS = 'C',
    /// <summary>
    /// German (DE)
    /// </summary>
    [Description("German (DE)")] DE = 'D',
    /// <summary>
    /// English (EN)
    /// </summary>
    [Description("English (EN)")] EN = 'E',
    /// <summary>
    /// French (FR)
    /// </summary>
    [Description("French (FR)")] FR = 'F',
    /// <summary>
    /// Greek (EL)
    /// </summary>
    [Description("Greek (EL)")] EL = 'G',
    /// <summary>
    /// Hungarian (HU)
    /// </summary>
    [Description("Hungarian (HU)")] HU = 'H',
    /// <summary>
    /// Italian (IT)
    /// </summary>
    [Description("Italian (IT)")] IT = 'I',
    /// <summary>
    /// Japanese (JA)
    /// </summary>
    [Description("Japanese (JA)")] JA = 'J',
    /// <summary>
    /// Danish (DA)
    /// </summary>
    [Description("Danish (DA)")] DA = 'K',
    /// <summary>
    /// Polish (PL)
    /// </summary>
    [Description("Polish (PL)")] PL = 'L',
    /// <summary>
    /// Chinese trad. (ZF)
    /// </summary>
    [Description("Chinese trad. (ZF)")] ZF = 'M',
    /// <summary>
    /// Dutch (NL)
    /// </summary>
    [Description("Dutch (NL)")] NL = 'N',
    /// <summary>
    /// Norwegian (NO)
    /// </summary>
    [Description("Norwegian (NO)")] NO = 'O',
    /// <summary>
    /// Portuguese (PT)
    /// </summary>
    [Description("Portuguese (PT)")] PT = 'P',
    /// <summary>
    /// Slovak (SK)
    /// </summary>
    [Description("Slovak (SK)")] SK = 'Q',
    /// <summary>
    /// Russian (RU)
    /// </summary>
    [Description("Russian (RU)")] RU = 'R',
    /// <summary>
    /// Spanish (ES)
    /// </summary>
    [Description("Spanish (ES)")] ES = 'S',
    /// <summary>
    /// Turkish (TR)
    /// </summary>
    [Description("Turkish (TR)")] TR = 'T',
    /// <summary>
    /// Finnish (FI)
    /// </summary>
    [Description("Finnish (FI)")] FI = 'U',
    /// <summary>
    /// Swedish (SV)
    /// </summary>
    [Description("Swedish (SV)")] SV = 'V',
    /// <summary>
    /// Bulgarian (BG)
    /// </summary>
    [Description("Bulgarian (BG)")] BG = 'W',
    /// <summary>
    /// Lithuanian (LT)
    /// </summary>
    [Description("Lithuanian (LT)")] LT = 'X',
    /// <summary>
    /// Latvian (LV)
    /// </summary>
    [Description("Latvian (LV)")] LV = 'Y',
    /// <summary>
    /// Customer reserve (Z1)
    /// </summary>
    [Description("Customer reserve (Z1)")] Z1 = 'Z',
    /// <summary>
    /// Afrikaans (AF)
    /// </summary>
    [Description("Afrikaans (AF)")] AF = 'a',
    /// <summary>
    /// Icelandic (IS)
    /// </summary>
    [Description("Icelandic (IS)")] IS = 'b',
    /// <summary>
    /// Catalan (CA)
    /// </summary>
    [Description("Catalan (CA)")] CA = 'c',
    /// <summary>
    /// Serbian (Latin) (SH)
    /// </summary>
    [Description("Serbian (Latin) (SH)")] SH = 'd',
    /// <summary>
    /// Indonesian (ID)
    /// </summary>
    [Description("Indonesian (ID)")] ID = 'i',
    /// <summary>
    /// Hindi (HI)
    /// </summary>
    [Description("Hindi (HI)")] HI = '묩',
    /// <summary>
    /// Kazakh (KK)
    /// </summary>
    [Description("Kazakh (KK)")] KK = '뱋',
    /// <summary>
    /// Vietnamese (VI)
    /// </summary>
    [Description("Vietnamese (VI)")] VI = '쁩',

    /// <summary>
    /// Not defined
    /// </summary>
    [Description("Not defined")] None = 0
}

public static class SAPLanguageExtensions {
    private static readonly Dictionary<string, SAPLanguage> langCodeToEnumMap;

    static SAPLanguageExtensions() {
        langCodeToEnumMap = new Dictionary<string, SAPLanguage>(StringComparer.OrdinalIgnoreCase);
        foreach(SAPLanguage language in Enum.GetValues<SAPLanguage>()) {
            char langChar = (char)language;
            if(langChar != 0) // Exclude the 'None' value with char code 0
                langCodeToEnumMap[langChar.ToString()] = language;
        }
    }

    public static string? GetDescription(this SAPLanguage lang) {
        Type type = lang.GetType();
        MemberInfo[] memInfo = type.GetMember(lang.ToString());

        if(memInfo.Length > 0) {
            object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if(attrs.Length > 0)
                return ((DescriptionAttribute)attrs[0]).Description;
        }
        return "No description";
    }

    public static string? GetDescription(this string? langCode) => String.IsNullOrEmpty(langCode)
            ? null
            : !langCodeToEnumMap.TryGetValue(langCode, out SAPLanguage language) ? "No description" : GetDescription(language);

    public static SAPLanguage FromCode(string lang) {
        lang = lang.Replace("\0", String.Empty);
        if(String.IsNullOrEmpty(lang)) return SAPLanguage.None;

        if(langCodeToEnumMap.TryGetValue(lang, out var language))
            return language;

        // Handle special cases or log unknown language
        return SAPLanguage.None;
    }

    public static string ToCode(this SAPLanguage lang) => ((char)lang).ToString();
}