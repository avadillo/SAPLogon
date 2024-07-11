using System.ComponentModel;
using System.Text;

namespace SAPTools.LogonTicket.Extensions;

public enum SAPCodepage : ushort {
    //Fron TCP00 + TCP00A
    [Description("SAP internal, like ISO 8859-1 (iso-8859-1)")] ISO8859_1 = 1100,
    [Description("7-Bit USA ASCII  pur (US-ASCII)")] ASCII = 1101,
    [Description("IBM PC Multilingual 850 (ibm850)")] Cp850 = 1103,
    [Description("ISO 8859-1  Ausgabe (iso-8859-1)")] ISO8859_1a = 1140,
    [Description("Microsoft 1252, Superset of ISO 8859-1 (windows-1252)")] Cp1252 = 1160,
    [Description("ISO 8859-15  (Latin-9) (iso-8859-15)")] ISO8859_15 = 1164,
    [Description("Transliteration from Latin-2 (...) to Latin-1 (iso-8859-1)")] ISO8859_1b = 1180,
    [Description("SAP internal, like ISO 8859-2 (iso-8859-2)")] ISO8859_2 = 1401,
    [Description("IBM PC Code Page 852 (Latin 2 for OS/2) (ibm852)")] Cp852 = 1403,
    [Description("Microsoft Windows 1250 for Central Europe (windows-1250)")] Cp1250 = 1404,
    [Description("SAP-internal, like ISO 8859-5 (iso-8859-5)")] ISO8859_5 = 1500,
    [Description("IBM PC-866 (ibm866)")] Cp866 = 1503,
    [Description("Microsoft Windows 1251 mit kyrillisch (windows-1251)")] Cp1251 = 1504,
    [Description("Ukrainian KOI8-U (koi8-u)")] KOI8_U = 1509,
    [Description("Russian KOI8-R (koi8-r)")] KOI8_R = 1510,
    [Description("SAP-internal, like ISO 8859-9 (Latin-5) (iso-8859-9)")] ISO8859_9 = 1610,
    [Description("MS Windows 1254  wie ISO 8859-9 (Latin Nr.5) (windows-1254)")] Cp1254 = 1614,
    [Description("SAP-internal, like ISO 8859-7  (Greek) (iso-8859-7)")] ISO8859_7 = 1700,
    [Description("MS-Windows griechisch (windows-1253)")] Cp1253 = 1704,
    [Description("SAP-internal, like ISO 8859-8     (Hebrew) (iso-8859-8-i)")] ISO8859_8 = 1800,
    [Description("Microsoft Windows 1255 for Hebrew (windows-1255)")] Cp1255 = 1804,
    [Description("SAP-internal, like ISO 8859-4  (Litauian) (iso-8859-4)")] ISO8859_4 = 1900,
    [Description("Microsoft Windows 1257 fuer litauisch (f. Inhalte) (windows-1257)")] Cp1257 = 1904,
    [Description("UTF-16BE Unicode / ISO/IEC 10646 (utf-16be)")] UnicodeBigUnmarked = 4102,
    [Description("UTF-16LE Unicode / ISO/IEC 10646 (utf-16le)")] UnicodeLittleUnmarked = 4103,
    [Description("Unicode UTF-8 (utf-8)")] UTF8 = 4110,
    [Description("Printer Fujitsu VSPxxxx FNP series Unicode UTF-8 (utf-8)")] UTF_8 = 4310,
    [Description("Shift JIS (based on Microsoft CP932) (shift_jis)")] MS932 = 8000,
    [Description("Shift JIS for front-ends (shift_jis)")] MS932_FE = 8004,
    [Description("EUC Extended Unix Codepage (euc-jp)")] EUC_JP = 8100,
    [Description("JIS (iso-2022-jp)")] ISO2022JP = 8200,
    [Description("Traditional Chinese (Big5, based on MS CP950) (big5)")] MS950 = 8300,
    [Description("Traditional Chinese for front-ends (big5)")] MS950_FE = 8304,
    [Description("Simplified Chinese (based on GB2312-EUC, MS CP936) (GBK)")] GBK = 8400,
    [Description("Simplified Chinese for front-ends (GBK)")] GBK_FE = 8404,
    [Description("Simplified Chinese (based on GB-2312-80,EUC) (gb2312)")] EUC_CN = 8450,
    [Description("Koreanisch (based on KSC5601-1992, MS CP949) (EUC-KR)")] MS949 = 8500,
    [Description("Korean for front-ends (EUC-KR)")] MS949_FE = 8504,
    [Description("Thai Codepage ISO988/2533 (tis-620)")] TIS620 = 8600,
    [Description("Microsoft Windows 874 for Thai (windows-874)")] MS874 = 8604,
    [Description("ISO8859-6 Arabic for system code page (iso-8859-6)")] ISO8859_6 = 8700,
    [Description("MS 1256 Arabic Windows (windows-1256)")] Cp1256 = 8704,
    [Description("Microsoft 1258, Vietnamese with combining chars (windows-1258)")] Cp1258 = 8805,
    [Description("Unknown SAP Code Page")] Unknown = 0 // Default value for unknown codepage
}

public static class SAPCodepageExtensions {
    public static string GetDescription(this SAPCodepage encoding) {
        var type = encoding.GetType();
        var memberInfo = type.GetMember(encoding.ToString());

        if(memberInfo.Length > 0) {
            object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if(attributes.Length > 0)
                return ((DescriptionAttribute)attributes[0]).Description;
        }
        return encoding.ToString();
    }

    public static SAPCodepage FromCode(string code) {
        int number = Int32.TryParse(code, out number) ? number : 0;
        return Enum.IsDefined(typeof(SAPCodepage), number) ?
            (SAPCodepage)number : SAPCodepage.Unknown;
    }

    public static string ToCode(SAPCodepage codepage) =>
        codepage.GetHashCode().ToString();

    public static Encoding GetEncoding(SAPCodepage codepage) =>
        codepage switch {
            SAPCodepage.UnicodeLittleUnmarked => Encoding.Unicode,
            SAPCodepage.UnicodeBigUnmarked => Encoding.BigEndianUnicode,
            SAPCodepage.UTF8 => Encoding.UTF8,
            SAPCodepage.UTF_8 => Encoding.UTF8,
            _ => Encoding.ASCII,
        };
}