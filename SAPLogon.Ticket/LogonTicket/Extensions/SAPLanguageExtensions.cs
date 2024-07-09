using System;
using System.ComponentModel;
using System.Reflection;

namespace SAPTools.LogonTicket.Extensions;

public enum SAPLanguage {
    [Description("Serbian (SR)")] SR = '0',
    [Description("Chinese (ZH)")] ZH = '1',
    [Description("Thai (TH)")] TH = '2',
    [Description("Korean (KO)")] KO = '3',
    [Description("Romanian (RO)")] RO = '4',
    [Description("Slovenian (SL)")] SL = '5',
    [Description("Croatian (HR)")] HR = '6',
    [Description("Malay (MS)")] MS = '7',
    [Description("Ukrainian (UK)")] UK = '8',
    [Description("Estonian (ET)")] ET = '9',
    [Description("Arabic (AR)")] AR = 'A',
    [Description("Hebrew (HE)")] HE = 'B',
    [Description("Czech (CS)")] CS = 'C',
    [Description("German (DE)")] DE = 'D',
    [Description("English (EN)")] EN = 'E',
    [Description("French (FR)")] FR = 'F',
    [Description("Greek (EL)")] EL = 'G',
    [Description("Hungarian (HU)")] HU = 'H',
    [Description("Italian (IT)")] IT = 'I',
    [Description("Japanese (JA)")] JA = 'J',
    [Description("Danish (DA)")] DA = 'K',
    [Description("Polish (PL)")] PL = 'L',
    [Description("Chinese trad. (ZF)")] ZF = 'M',
    [Description("Dutch (NL)")] NL = 'N',
    [Description("Norwegian (NO)")] NO = 'O',
    [Description("Portuguese (PT)")] PT = 'P',
    [Description("Slovak (SK)")] SK = 'Q',
    [Description("Russian (RU)")] RU = 'R',
    [Description("Spanish (ES)")] ES = 'S',
    [Description("Turkish (TR)")] TR = 'T',
    [Description("Finnish (FI)")] FI = 'U',
    [Description("Swedish (SV)")] SV = 'V',
    [Description("Bulgarian (BG)")] BG = 'W',
    [Description("Lithuanian (LT)")] LT = 'X',
    [Description("Latvian (LV)")] LV = 'Y',
    [Description("Customer reserve (Z1)")] Z1 = 'Z',
    [Description("Afrikaans (AF)")] AF = 'a',
    [Description("Icelandic (IS)")] IS = 'b',
    [Description("Catalan (CA)")] CA = 'c',
    [Description("Serbian (Latin) (SH)")] SH = 'd',
    [Description("Indonesian (ID)")] ID = 'i',
    [Description("Hindi (HI)")] HI = '묩',
    [Description("Kazakh (KK)")] KK = '뱋',
    [Description("Vietnamese (VI)")] VI = '쁩',
    [Description("No description")] None = 0
}

public static class SAPLanguageExtensions
{
    private static readonly Dictionary<string, SAPLanguage> langCodeToEnumMap;

    static SAPLanguageExtensions() {
        langCodeToEnumMap = new Dictionary<string, SAPLanguage>(StringComparer.OrdinalIgnoreCase);
        foreach(SAPLanguage language in Enum.GetValues(typeof(SAPLanguage))) {
            char langChar = (char)language;
            if(langChar != 0) // Exclude the 'None' value with char code 0
                langCodeToEnumMap[langChar.ToString()] = language;
        }
    }

    public static string GetDescription(this SAPLanguage lang)
    {
        var type = lang.GetType();
        var memInfo = type.GetMember(lang.ToString());

        if (memInfo.Length > 0) {
            object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs.Length > 0)
                return ((DescriptionAttribute)attrs[0]).Description;
        }
        return "No description";
    }

    public static SAPLanguage FromString(string lang) {
        lang = lang.Replace("\0", String.Empty);
        if(String.IsNullOrEmpty(lang)) return SAPLanguage.None;

        if(langCodeToEnumMap.TryGetValue(lang, out var language)) 
            return language;

        // Handle special cases or log unknown language
        return SAPLanguage.None;
    }

    public static SAPLanguage FromDescription(string description) {
        foreach (SAPLanguage language in Enum.GetValues(typeof(SAPLanguage))) {
            if (description.Equals(language.GetDescription(), StringComparison.OrdinalIgnoreCase))
                return language;
        }

        // Handle special cases or log unknown language
        return SAPLanguage.None;
    }

    public static string ToCode(this SAPLanguage lang) => ((char)lang).ToString();
}