using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;

namespace SAPTools.Ticket.Extensions;

public enum InfoUnitID : byte {
    /// <summary>
    /// ABAP User ID (BNAME)
    /// </summary>
    [Description("User")] User = 0x01,
    /// <summary>
    /// Issuing System Client (MANDT)
    /// </summary>
    [Description("Issuing System Client")] CreateClient = 0x02,
    /// <summary>
    /// Issuing System ID (SYSID)
    /// </summary>
    [Description("Issuing System ID")] CreateSID = 0x03,
    /// <summary>
    /// Creation Time (UTC)
    /// </summary>
    [Description("Creation Time")] CreateTime = 0x04,
    /// <summary>
    /// Valid Time (H) (in hours)
    /// </summary>
    [Description("Valid Time (H)")] ValidTimeInH = 0x05,
    /// <summary>
    /// Valid Time (M) (in minutes)
    /// </summary>
    [Description("Valid Time (M)")] ValidTimeInM = 0x07,
    /// <summary>
    /// Is it RFC? 'X'/ ' '
    [Description("Is RFC?")] RFC = 0x06,
    /// <summary>
    /// Ticket Flags (byte)
    /// </summary>
    [Description("Flags")] Flags = 0x8,
    /// <summary>
    /// SAP Logon Language (just the language code)
    /// </summary>
    [Description("Language")] Language = 0x09,
    // UTF InfoUnits are only informational and are not used for authentication
    /// <summary>
    /// ABAP User ID (BNAME) (UTF8 - Not Used)
    /// </summary>
    [Description("User (UTF8)")] UTF8_User = 0x0A,
    /// <summary>
    /// Issuing System Client (MANDT) (UTF8 - Not Used)
    /// </summary>
    [Description("Issuing System Client (UTF8)")] UTF8_CreateClient = 0x0B,
    /// <summary>
    /// Issuing System ID (SYSID) (UTF8 - Not Used)
    /// </summary>
    [Description("Issuing System ID (UTF8)")] UTF8_CreateSID = 0x0C,
    /// <summary>
    /// Creation Time (UTF8 - Not Used)
    /// </summary>
    [Description("Creation Time (UTF8)")] UTF8_CreateTime = 0x0D,
    /// <summary>
    /// SAP Logon Language (UTF8 - Not Used)
    /// </summary>
    [Description("Language (UTF8)")] UTF8_Language = 0x0E,
    // Assertion Tickets need a recipient client and name
    /// <summary>
    /// Recipient System ID for Assertion Tickets
    /// </summary>
    [Description("Recipient System Client")] RecipientClient = 0x0F,
    /// <summary>
    /// Recurrent System Client for Assertion Tickets
    /// </summary>
    [Description("Recipient System Client")] RecipientSID = 0x10,
    /// <summary>
    /// Portal User
    /// </summary>
    [Description("Portal User")] PortalUser = 0x20,
    /// <summary>
    /// Authentication Scheme
    /// </summary>
    [Description("Authentication Scheme")] AuthScheme = 0x88,
    /// <summary>
    /// Four Byte ID (RESERVED)
    /// </summary>
    [Description("Four Byte ID")] FourByteID = 0xFE,
    /// <summary>
    /// PKCS7 Signature
    /// </summary>
    [Description("Signature")] Signature = 0xFF,
    [Description("Not Defined")] NotDefined = 0x00
}

[Flags]
public enum InfoUnitFlags : byte {
    [Description("No Flags ")] None = 0x00,
    /// <summary>
    /// Do not store the tickets in the cache: use them only once.
    /// </summary>
    [Description("Do not store in Ticket Cache ")] DoNotCacheTicket = 0x01
}

public static class InfoUnitExtensions {
    public enum InfoUnitType : byte {
        String,
        StringUTF8,
        StringASCII,
        DateString,
        DateStringUTF8,
        LangString,
        LangStringUTF8,
        UnsignedInt,
        Byte,
        ByteArray,
        Signature,
    }

    public const string CreationDateFormat = "yyyyMMddHHmm";

    public static InfoUnitType GetInfoUnitType(this InfoUnitID id) =>
      id switch {
          InfoUnitID.User or InfoUnitID.CreateClient or InfoUnitID.CreateSID => InfoUnitType.String,
          InfoUnitID.UTF8_User or InfoUnitID.UTF8_CreateClient or InfoUnitID.UTF8_CreateSID => InfoUnitType.StringUTF8,
          InfoUnitID.Language => InfoUnitType.LangString,
          InfoUnitID.UTF8_Language => InfoUnitType.LangStringUTF8,
          InfoUnitID.CreateTime => InfoUnitType.DateString,
          InfoUnitID.UTF8_CreateTime => InfoUnitType.DateStringUTF8,
          InfoUnitID.RecipientClient or InfoUnitID.RecipientSID or InfoUnitID.PortalUser or InfoUnitID.AuthScheme or
          InfoUnitID.RFC => InfoUnitType.StringASCII,
          InfoUnitID.ValidTimeInH or InfoUnitID.ValidTimeInM => InfoUnitType.UnsignedInt,
          InfoUnitID.Flags => InfoUnitType.Byte,
          InfoUnitID.FourByteID => InfoUnitType.ByteArray,
          InfoUnitID.Signature => InfoUnitType.Signature,
          _ => throw new InvalidOperationException("Unknown InfoUnitID")
      };

    public static InfoUnitType GetInfoUnitType(this InfoUnit iu) => GetInfoUnitType(iu.ID);

    public static InfoUnitID FromByte(byte value) =>
        Enum.IsDefined(typeof(InfoUnitID), value) ? (InfoUnitID)value
            : throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is not defined in InfoUnitID enum.");

    public static InfoUnitID FromString(string value) =>
        Enum.TryParse(value, out InfoUnitID result) ? result
            : throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is not defined in InfoUnitID enum.");

    public static Encoding DetermineEncoding(this InfoUnitID id, Encoding enc) => id.GetInfoUnitType() switch {
        InfoUnitType.String or InfoUnitType.DateString or InfoUnitType.LangString => enc,
        InfoUnitType.StringUTF8 or InfoUnitType.DateStringUTF8 or InfoUnitType.LangStringUTF8 => Encoding.UTF8,
        InfoUnitType.StringASCII => Encoding.ASCII,
        _ => throw new InvalidOperationException($"InfoUnit {id} is not a string unit.")
    };

    public static Encoding DetermineEncoding(this InfoUnit iu, Encoding enc) => DetermineEncoding(iu.ID, enc);

    public static Encoding DetermineEncoding(this InfoUnitID id) =>
        id.GetInfoUnitType() switch {
            InfoUnitType.StringASCII => Encoding.ASCII,
            InfoUnitType.StringUTF8 or InfoUnitType.DateStringUTF8 or InfoUnitType.LangStringUTF8 => Encoding.UTF8,
            InfoUnitType.String or InfoUnitType.DateString or InfoUnitType.LangString => throw new InvalidOperationException($"InfoUnit {id} needs an encoding"),
            _ => throw new InvalidOperationException($"InfoUnit {id} is not a string unit.")
        };

    public static Encoding DetermineEncoding(this InfoUnit iu) => DetermineEncoding(iu.ID);

    public static string ToString(this InfoUnit iu, Encoding enc) => iu.DetermineEncoding(enc).GetString(iu.Content);

    public static object? GetValue(this InfoUnit iu, Encoding enc) => iu.GetInfoUnitType() switch {
        InfoUnitType.String => iu.ToString(enc),
        InfoUnitType.StringUTF8 => Encoding.UTF8.GetString(iu.Content),
        InfoUnitType.StringASCII => Encoding.ASCII.GetString(iu.Content),
        InfoUnitType.DateString or InfoUnitType.DateStringUTF8 => DateTime.ParseExact(iu.ToString(enc), CreationDateFormat, null),
        InfoUnitType.LangString or InfoUnitType.LangStringUTF8 => SAPLanguageExtensions.FromCode(iu.ToString(enc)),
        InfoUnitType.Byte => iu.Content[0],
        InfoUnitType.UnsignedInt => BitConverter.IsLittleEndian ? BitConverter.ToUInt32(iu.Content.Reverse().ToArray(), 0) : BitConverter.ToUInt32(iu.Content, 0),
        InfoUnitType.ByteArray => iu.Content,
        InfoUnitType.Signature => iu.DecodeSignedCms(),
        _ => throw new InvalidOperationException("Unknown InfoUnitType"),
    };

    private static SignedCms? DecodeSignedCms(this InfoUnit iu) {
        SignedCms cms = new();
        try {
            cms.Decode(iu.Content);
        } catch(CryptographicException) {
            return null;
        }
        return cms;
    }
    public static List<InfoUnit> ParseInfoUnits(Span<byte> ticket) {
        List<InfoUnit> infoUnits = [];
        int offset = 0;
        while(offset < ticket.Length) {
            InfoUnitID id = FromByte(ticket[offset++]);
            ushort length = (ushort)((ticket[offset++] << 8) | ticket[offset++]);
            byte[] content = ticket.Slice(offset, Math.Min(length, ticket.Length - offset)).ToArray();
            offset += length;
            infoUnits.Add(new InfoUnit(id, content));
        }
        return infoUnits;
    }
}
