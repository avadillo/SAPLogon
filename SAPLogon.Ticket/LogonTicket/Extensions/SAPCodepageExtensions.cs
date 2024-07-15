using System.ComponentModel;
using System.Text;

namespace SAPTools.LogonTicket.Extensions;

/// <summary>
/// Defines the SAP code pages
/// </summary>
public enum SAPCodepage : ushort {
    //Fron TCP00 + TCP00A
    /// <summary>
    ///SAP internal, like ISO 8859-1 (iso-8859-1)
    /// </summary>
    [Description("SAP internal, like ISO 8859-1 (iso-8859-1)")] ISO8859_1 = 1100,
    /// <summary>
    ///7-Bit USA ASCII  pur (US-ASCII)
    /// </summary>
    [Description("7-Bit USA ASCII  pur (US-ASCII)")] ASCII = 1101,
    /// <summary>
    ///IBM PC Multilingual 850 (ibm850)
    /// </summary>
    [Description("IBM PC Multilingual 850 (ibm850)")] Cp850 = 1103,
    /// <summary>
    ///ISO 8859-1 Ausgabe (iso-8859-1)
    /// </summary>
    [Description("ISO 8859-1 Ausgabe (iso-8859-1)")] ISO8859_1a = 1140,
    /// <summary>
    ///Microsoft 1252, Superset of ISO 8859-1 (windows-1252)
    /// </summary>
    [Description("Microsoft 1252, Superset of ISO 8859-1 (windows-1252)")] Cp1252 = 1160,
    /// <summary>
    ///ISO 8859-15  (Latin-9) (iso-8859-15)
    /// </summary>
    [Description("ISO 8859-15  (Latin-9) (iso-8859-15)")] ISO8859_15 = 1164,
    /// <summary>
    ///Transliteration from Latin-2 (...) to Latin-1 (iso-8859-1)
    /// </summary>
    [Description("Transliteration from Latin-2 (...) to Latin-1 (iso-8859-1)")] ISO8859_1b = 1180,
    /// <summary>
    ///SAP internal, like ISO 8859-2 (iso-8859-2)
    /// </summary>
    [Description("SAP internal, like ISO 8859-2 (iso-8859-2)")] ISO8859_2 = 1401,
    /// <summary>
    ///IBM PC Code Page 852 (Latin 2 for OS/2) (ibm852)
    /// </summary>
    [Description("IBM PC Code Page 852 (Latin 2 for OS/2) (ibm852)")] Cp852 = 1403,
    /// <summary>
    ///Microsoft Windows 1250 for Central Europe (windows-1250)
    /// </summary>
    [Description("Microsoft Windows 1250 for Central Europe (windows-1250)")] Cp1250 = 1404,
    /// <summary>
    ///SAP-internal, like ISO 8859-5 (iso-8859-5)
    /// </summary>
    [Description("SAP-internal, like ISO 8859-5 (iso-8859-5)")] ISO8859_5 = 1500,
    /// <summary>
    ///IBM PC-866 (ibm866)
    /// </summary>
    [Description("IBM PC-866 (ibm866)")] Cp866 = 1503,
    /// <summary>
    ///Microsoft Windows 1251 mit kyrillisch (windows-1251)
    /// </summary>
    [Description("Microsoft Windows 1251 mit kyrillisch (windows-1251)")] Cp1251 = 1504,
    /// <summary>
    ///Ukrainian KOI8-U (koi8-u)
    /// </summary>
    [Description("Ukrainian KOI8-U (koi8-u)")] KOI8_U = 1509,
    /// <summary>
    ///Russian KOI8-R (koi8-r)
    /// </summary>
    [Description("Russian KOI8-R (koi8-r)")] KOI8_R = 1510,
    /// <summary>
    ///SAP-internal, like ISO 8859-9 (Latin-5) (iso-8859-9)
    /// </summary>
    [Description("SAP-internal, like ISO 8859-9 (Latin-5) (iso-8859-9)")] ISO8859_9 = 1610,
    /// <summary>
    ///MS Windows 1254  wie ISO 8859-9 (Latin Nr.5) (windows-1254)
    /// </summary>
    [Description("MS Windows 1254  wie ISO 8859-9 (Latin Nr.5) (windows-1254)")] Cp1254 = 1614,
    /// <summary>
    ///SAP-internal, like ISO 8859-7  (Greek) (iso-8859-7)
    /// </summary>
    [Description("SAP-internal, like ISO 8859-7  (Greek) (iso-8859-7)")] ISO8859_7 = 1700,
    /// <summary>
    ///MS-Windows griechisch (windows-1253)
    /// </summary>
    [Description("MS-Windows griechisch (windows-1253)")] Cp1253 = 1704,
    /// <summary>
    ///SAP-internal, like ISO 8859-8     (Hebrew) (iso-8859-8-i)
    /// </summary>
    [Description("SAP-internal, like ISO 8859-8     (Hebrew) (iso-8859-8-i)")] ISO8859_8 = 1800,
    /// <summary>
    ///Microsoft Windows 1255 for Hebrew (windows-1255)
    /// </summary>
    [Description("Microsoft Windows 1255 for Hebrew (windows-1255)")] Cp1255 = 1804,
    /// <summary>
    ///SAP-internal, like ISO 8859-4  (Litauian) (iso-8859-4)
    /// </summary>
    [Description("SAP-internal, like ISO 8859-4  (Litauian) (iso-8859-4)")] ISO8859_4 = 1900,
    /// <summary>
    ///Microsoft Windows 1257 fuer litauisch (f. Inhalte) (windows-1257)
    /// </summary>
    [Description("Microsoft Windows 1257 fuer litauisch (f. Inhalte) (windows-1257)")] Cp1257 = 1904,
    /// <summary>
    ///UTF-16BE Unicode / ISO/IEC 10646 (utf-16be)
    /// </summary>
    [Description("UTF-16BE Unicode / ISO/IEC 10646 (utf-16be)")] UnicodeBigUnmarked = 4102,
    /// <summary>
    ///UTF-16LE Unicode / ISO/IEC 10646 (utf-16le)
    /// </summary>
    [Description("UTF-16LE Unicode / ISO/IEC 10646 (utf-16le)")] UnicodeLittleUnmarked = 4103,
    /// <summary>
    ///Unicode UTF-8 (utf-8)
    /// </summary>
    [Description("Unicode UTF-8 (utf-8)")] UTF8 = 4110,
    /// <summary>
    ///Shift JIS (based on Microsoft CP932) (shift_jis)
    /// </summary>
    [Description("Shift JIS (based on Microsoft CP932) (shift_jis)")] MS932 = 8000,
    /// <summary>
    ///Shift JIS for front-ends (shift_jis)
    /// </summary>
    [Description("Shift JIS for front-ends (shift_jis)")] MS932_FE = 8004,
    /// <summary>
    ///EUC Extended Unix Codepage (euc-jp)
    /// </summary>
    [Description("EUC Extended Unix Codepage (euc-jp)")] EUC_JP = 8100,
    /// <summary>
    ///JIS (iso-2022-jp)
    /// </summary>
    [Description("JIS (iso-2022-jp)")] ISO2022JP = 8200,
    /// <summary>
    ///Traditional Chinese (Big5, based on MS CP950) (big5)
    /// </summary>
    [Description("Traditional Chinese (Big5, based on MS CP950) (big5)")] MS950 = 8300,
    /// <summary>
    ///Traditional Chinese for front-ends (big5)
    /// </summary>
    [Description("Traditional Chinese for front-ends (big5)")] MS950_FE = 8304,
    /// <summary>
    ///Simplified Chinese (based on GB2312-EUC, MS CP936) (GBK)
    /// </summary>
    [Description("Simplified Chinese (based on GB2312-EUC, MS CP936) (GBK)")] GBK = 8400,
    /// <summary>
    ///Simplified Chinese for front-ends (GBK)
    /// </summary>
    [Description("Simplified Chinese for front-ends (GBK)")] GBK_FE = 8404,
    /// <summary>
    ///Simplified Chinese (based on GB-2312-80,EUC) (gb2312)
    /// </summary>
    [Description("Simplified Chinese (based on GB-2312-80,EUC) (gb2312)")] EUC_CN = 8450,
    /// <summary>
    ///Koreanisch (based on KSC5601-1992, MS CP949) (EUC-KR)
    /// </summary>
    [Description("Koreanisch (based on KSC5601-1992, MS CP949) (EUC-KR)")] MS949 = 8500,
    /// <summary>
    ///Korean for front-ends (EUC-KR)
    /// </summary>
    [Description("Korean for front-ends (EUC-KR)")] MS949_FE = 8504,
    /// <summary>
    ///Thai Codepage ISO988/2533 (tis-620)
    /// </summary>
    [Description("Thai Codepage ISO988/2533 (tis-620)")] TIS620 = 8600,
    /// <summary>
    ///Microsoft Windows 874 for Thai (windows-874)
    /// </summary>
    [Description("Microsoft Windows 874 for Thai (windows-874)")] MS874 = 8604,
    /// <summary>
    ///ISO8859-6 Arabic for system code page (iso-8859-6)
    /// </summary>
    [Description("ISO8859-6 Arabic for system code page (iso-8859-6)")] ISO8859_6 = 8700,
    /// <summary>
    ///MS 1256 Arabic Windows (windows-1256)
    /// </summary>
    [Description("MS 1256 Arabic Windows (windows-1256)")] Cp1256 = 8704,
    /// <summary>
    ///Microsoft 1258, Vietnamese with combining chars (windows-1258)
    /// </summary>
    [Description("Microsoft 1258, Vietnamese with combining chars (windows-1258)")] Cp1258 = 8805,

    /// <summary>
    /// Unkown codepage
    /// </summary> 
    Unknown = 0 // Default value for unknown codepage
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
        ushort number = (ushort)(UInt16.TryParse(code, out number) ? number : 0);
        return Enum.IsDefined(typeof(SAPCodepage), number) ?
            (SAPCodepage)number : SAPCodepage.Unknown;
    }

    public static SAPCodepage FromCode(Span<byte> code) =>  FromCode(Encoding.ASCII.GetString(code));

    public static string ToCode(SAPCodepage codepage) => codepage.GetHashCode().ToString();

    public static Encoding GetEncoding(SAPCodepage codepage) =>
        codepage switch {
            SAPCodepage.UnicodeLittleUnmarked => Encoding.Unicode,
            SAPCodepage.UnicodeBigUnmarked => Encoding.BigEndianUnicode,
            SAPCodepage.UTF8 => Encoding.UTF8,
            _ => Encoding.ASCII,
        };
}