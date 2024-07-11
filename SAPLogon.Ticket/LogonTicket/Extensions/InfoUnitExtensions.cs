using System.ComponentModel;
using System.Text;

namespace SAPTools.LogonTicket.Extensions;

public enum InfoUnitID : byte {
    [Description("User")] User = 0x01,
    [Description("Issuing System Client")] CreateClient = 0x02,
    [Description("Issuing System ID")] CreateSID = 0x03,
    [Description("Creation Time")] CreateTime = 0x04,
    [Description("Valid Time (H)")] ValidTimeInH = 0x05,
    [Description("Valid Time (M)")] ValidTimeInM = 0x07,
    [Description("Is RFC?")] RFC = 0x06,
    [Description("Flags")] Flags = 0x8,
    [Description("Language")] Language = 0x09,
    // UTF InfoUnits are only informational and are not used for authentication
    [Description("User (UTF8)")] UTF8_User = 0x0A,
    [Description("Issuing System Client (UTF8)")] UTF8_CreateClient = 0x0B,
    [Description("Issuing System ID (UTF8)")] UTF8_CreateSID = 0x0C,
    [Description("Creation Time (UTF8)")] UTF8_CreateTime = 0x0D,
    [Description("Language (UTF8)")] UTF8_Language = 0x0E,
    // Assertion Tickets need a recipient client and name
    [Description("Recipient System Client")] RecipientClient = 0x0F,
    [Description("Recipient System Client")] RecipientSID = 0x10,
    [Description("Portal User")] PortalUser = 0x20,
    [Description("Authentication Scheme")] AuthScheme = 0x88,
    [Description("Four Byte ID")] FourByteID = 0xFE,
    [Description("Signature")] Signature = 0xFF,
    [Description("Not Defined")] NotDefined = 0x00
}

public enum InfoUnitType : byte {
    String,
    StringUTF8,
    StringASCII,
    UnsignedInt,
    Byte,
    ByteArray,
}

[Flags]
public enum InfoUnitFlags : byte {
    [Description("No Flags ")] None = 0x00,
    [Description("Do not store in Ticket Cache ")] DoNotCacheTicket = 0x01
}

public static class InfoUnitExtensions {
    public const string DateFormat = "yyyyMMddHHmm";

    public static InfoUnitType GetInfoUnitType(this InfoUnitID id) =>
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