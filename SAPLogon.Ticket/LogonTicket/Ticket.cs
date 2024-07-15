using Microsoft.VisualBasic;
using SAPTools.LogonTicket.Extensions;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using static SAPTools.LogonTicket.Extensions.InfoUnitExtensions;

namespace SAPTools.LogonTicket;

public abstract class Ticket {
    /// <summary>
    /// ABAP User ID (BNAME) up to 12 characters
    /// </summary>
    public required string User { get; set; }
    /// <summary>
    /// Issuing System ID (SYSID) up to 8 characters 
    /// As defined in transaction STRUSTSSO2 
    /// </summary>
    public required string SysID { get; set; }
    /// <summary>
    /// Issuing System Client (MANDT) exactly three digits
    /// As defined in transaction STRUSTSSO2 
    /// </summary>
    public required string SysClient { get; set; }
    /// <summary>
    /// The subject of the certificate used to sign the ticket.
    /// </summary>
    public required string Subject { get; set; } 
    /// <summary>
    /// Validity time of the ticket in minutes.
    /// </summary>
    public virtual uint ValidTime { get; set; } 
    /// <summary>
    /// Include the certificate in the ticket.
    /// Not included by default.
    /// </summary>
    public bool IncludeCert { get; set; } = false;
    public bool IsRFC { get; set; } = false;

    private readonly SAPCodepage Codepage = SAPCodepage.UnicodeLittleUnmarked;

    protected List<InfoUnit> InfoUnits { get; set; } = [];
    private byte[] TicketContent { get; set; } = [];
    protected Encoding InternalEncoding = Encoding.ASCII;

    public string ToBase64 => SAPTools.Utils.Base64.Encode(TicketContent);
    public byte[] ToBytes => TicketContent;

    /// <summary>
    /// Create the ticket payload + signature.
    /// Returns the Base64 encoded ticket.
    /// </summary>
    /// <returns></returns>
    public string Create() {
        InternalEncoding = SAPCodepageExtensions.GetEncoding(Codepage);
        using MemoryStream payload = new(512);
        payload.WriteByte(2); // Tickets always start with a (byte)0x02
        payload.Write(Encoding.ASCII.GetBytes(SAPCodepageExtensions.ToCode(Codepage)));

        EncodeInfoUnits();
        foreach(var iu in InfoUnits) iu.WriteTo(payload);

        var signature = GetSignature(payload.ToArray());
        InfoUnit iuSignature = new(InfoUnitID.Signature, signature);
        iuSignature.WriteTo(payload);
        TicketContent = payload.ToArray();

        return SAPTools.Utils.Base64.Encode(TicketContent);
    }

    private SignedCms GetSignature(byte[] data) {
        using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        X509Certificate2 cert = UserCertificates.GetCertificateBySubject(Subject).Result;
        ContentInfo content = new(new Oid("1.2.840.113549.1.7.1"), data); // PKCS7
        SignedCms signedCms = new(content, true);
        CmsSigner signer = new(SubjectIdentifierType.IssuerAndSerialNumber, cert) {
            IncludeOption = IncludeCert ? X509IncludeOption.EndCertOnly : X509IncludeOption.None,
            SignedAttributes = { new Pkcs9SigningTime(DateTime.UtcNow) }
        };

        if(cert.PublicKey.Oid.Value == "1.2.840.10040.4.1") // DSA
            signer.DigestAlgorithm = new Oid("1.3.14.3.2.26"); // SHA1
        signedCms.ComputeSignature(signer);

        return signedCms;
    }

    protected virtual void EncodeInfoUnits() {
        InfoUnits.Clear();
        // The following InfoUnits need an encoding depending on the codepage
        InfoUnits.Add(new(InfoUnitID.User, User, InternalEncoding));
        InfoUnits.Add(new(InfoUnitID.CreateClient, SysClient, InternalEncoding));
        InfoUnits.Add(new(InfoUnitID.CreateSID, SysID, InternalEncoding));
        InfoUnits.Add(new(InfoUnitID.CreateTime, DateTime.UtcNow, InternalEncoding));

        // The following InfoUnits do not need an encoding
        InfoUnits.Add(new(InfoUnitID.ValidTimeInM, ValidTime % 60));
        if(ValidTime < 60) InfoUnits.Add(new(InfoUnitID.ValidTimeInH, ValidTime / 60));
        InfoUnits.Add(new(InfoUnitID.UTF8_User, User));
        if(IsRFC) InfoUnits.Add(new(InfoUnitID.RFC, "X"));
    }

    public static Ticket ParseTicket(string base64Ticket) {
        byte[] ticket = SAPTools.Utils.Base64.Decode(base64Ticket);
        return ParseTicket(ticket);
    }

    public static Ticket ParseTicket(byte[] ticket) {
        if(ticket[0] != 2) {
            throw new ArgumentException("Invalid ticket format.");
        }

        SAPCodepage codepage = SAPCodepageExtensions.FromCode(ticket.AsSpan(1, 4));
        Encoding encoding = SAPCodepageExtensions.GetEncoding(codepage);

        List<InfoUnit> infoUnits = InfoUnitExtensions.ParseInfoUnits(ticket.AsSpan(5, ticket.Length - 5));
        if(GetValue(infoUnits, InfoUnitID.Signature, encoding) is not SignedCms cms) 
            throw new ArgumentException("Ticket is not signed.");

        string subject = cms.SignerInfos.Count > 0 ? ((X509IssuerSerial)cms.SignerInfos[0].SignerIdentifier.Value!).IssuerName : "";

        return GetValue(infoUnits, InfoUnitID.Flags, encoding) is byte flags && flags == 1
            ? CreateAssertionTicket(infoUnits, encoding, subject)
            : CreateLogonTicket(infoUnits, encoding, subject);
    }

    private static AssertionTicket CreateAssertionTicket(List<InfoUnit> infoUnits, Encoding encoding, string subject) => new () {
        User = "", RcptSysClient = "", RcptSysID = "", SysClient = "", SysID = "", 
        Subject = subject,
        InfoUnits = infoUnits,
        InternalEncoding = encoding,
    };

    private static LogonTicket CreateLogonTicket(List<InfoUnit> infoUnits, Encoding encoding, string subject) => new () {
        User = "", SysClient = "", SysID = "",
        Subject = subject,
        InfoUnits = infoUnits,
        InternalEncoding = encoding,
    };


    public static object? GetValue(List<InfoUnit> infoUnits, InfoUnitID id, Encoding enc) =>
    infoUnits.FirstOrDefault(i => i.ID == id) is InfoUnit iu ? iu.GetValue(enc) : null;

    public object? GetValue(InfoUnitID id) => GetValue(InfoUnits, id, InternalEncoding);
}