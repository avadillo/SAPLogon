using System.ComponentModel;
using System.Text;

namespace SAPTools.LogonTicket.Extensions;

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
    public const string ValidDateFormat = "yyyyMMddHHmm";
    private enum InfoUnitType : byte {
        String,
        StringUTF8,
        StringASCII,
        UnsignedInt,
        Byte,
        ByteArray,
    }

    private static InfoUnitType GetInfoUnitType(this InfoUnitID id) =>
      id switch {
          InfoUnitID.User or
          InfoUnitID.CreateClient or
          InfoUnitID.CreateSID or
          InfoUnitID.CreateTime or
          InfoUnitID.Language => InfoUnitType.String,
          InfoUnitID.UTF8_User or
          InfoUnitID.UTF8_CreateClient or
          InfoUnitID.UTF8_CreateSID or
          InfoUnitID.UTF8_CreateTime or
          InfoUnitID.UTF8_Language => InfoUnitType.StringUTF8,
          InfoUnitID.RecipientClient or
          InfoUnitID.RecipientSID or
          InfoUnitID.PortalUser or
          InfoUnitID.AuthScheme or
          InfoUnitID.RFC => InfoUnitType.StringASCII,
          InfoUnitID.ValidTimeInH or
          InfoUnitID.ValidTimeInM => InfoUnitType.UnsignedInt,
          InfoUnitID.Flags => InfoUnitType.Byte,
          InfoUnitID.FourByteID or
          InfoUnitID.Signature => InfoUnitType.ByteArray,
          _ => throw new InvalidOperationException("Unknown InfoUnitID")
      };

    public static InfoUnitID FromByte(byte value) =>
        Enum.IsDefined(typeof(InfoUnitID), value) ? (InfoUnitID)value
            : throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is not defined in InfoUnitID enum.");

    public static InfoUnitID FromString(string value) =>
        Enum.TryParse(value, out InfoUnitID result) ? result
            : throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is not defined in InfoUnitID enum.");

    public static Encoding DetermineEncoding(InfoUnitID id, Encoding enc) =>
        id.GetInfoUnitType() switch {
            InfoUnitType.String => enc,
            InfoUnitType.StringUTF8 => Encoding.UTF8,
            InfoUnitType.StringASCII => Encoding.ASCII,
            _ => throw new InvalidOperationException($"InfoUnit {id} is not a string unit.")
        };

    public static Encoding DetermineEncoding(InfoUnitID id) =>
        id.GetInfoUnitType() switch {
            InfoUnitType.StringASCII => Encoding.ASCII,
            InfoUnitType.StringUTF8 => Encoding.UTF8,
            InfoUnitType.String => throw new InvalidOperationException($"InfoUnit {id} needs an encoding"),
            _ => throw new InvalidOperationException($"InfoUnit {id} is not a string unit.")
        };
}