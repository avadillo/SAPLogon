using Microsoft.VisualBasic;
using SAPTools.LogonTicket.Extensions;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
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
    /// Validity time of the ticket in minutes.
    /// </summary>
    public virtual uint ValidTime { get; set; } = 0;
    /// <summary>
    /// The certificate used to sign the ticket.
    /// </summary>
    public required X509Certificate2 Certificate { get; set; }
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

        ContentInfo content = new(new Oid("1.2.840.113549.1.7.1"), data); // PKCS7
        SignedCms signedCms = new(content, true);
        CmsSigner signer = new(SubjectIdentifierType.IssuerAndSerialNumber, Certificate) {
            IncludeOption = IncludeCert ? X509IncludeOption.EndCertOnly : X509IncludeOption.None,
            SignedAttributes = { new Pkcs9SigningTime(DateTime.UtcNow) }
        };

        if(Certificate.PublicKey.Oid.Value == "1.2.840.10040.4.1") // DSA
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

    protected class InfoUnit {
        public InfoUnitID ID { get; set; }
        public byte[] Content { get; set; }

        public InfoUnit(InfoUnitID id, byte[] data) =>
            (ID, Content) = (id, data);

        public InfoUnit(InfoUnitID id, byte data) =>
            (ID, Content) = (id, [data]);

        public InfoUnit(InfoUnitID id, string data) =>
            (ID, Content) = (id, DetermineEncoding(id).GetBytes(data));

        public InfoUnit(InfoUnitID id, string data, Encoding enc) =>
            (ID, Content) = (id, DetermineEncoding(id, enc).GetBytes(data));

        public InfoUnit(InfoUnitID id, SAPLanguage data, Encoding enc) =>
            (ID, Content) = (id, enc.GetBytes(SAPLanguageExtensions.ToCode(data)));

        public InfoUnit(InfoUnitID id, DateTime data, Encoding enc) =>
            (ID, Content) = (id, enc.GetBytes(data.ToString(ValidDateFormat)));

        public InfoUnit(InfoUnitID id, SignedCms data) =>
            (ID, Content) = (id, data.Encode());

        public InfoUnit(InfoUnitID id, uint data) =>
            (ID, Content) = (id, new byte[] {
            (byte)(data >> 24), (byte)(data >> 16),
            (byte)(data >> 8), (byte)data });

        public virtual void WriteTo(Stream @out) {
            // Ensure the content length does not exceed ushort.MaxValue - 3
            if(Content!.Length > UInt16.MaxValue - 3) throw new InvalidOperationException("Content is too large.");

            ushort totalLength = (ushort)(3 + Content.Length); // Total length calculation
            byte[] buffer = new byte[totalLength];

            buffer[0] = (byte)ID!; // ID
            buffer[1] = (byte)(Content.Length >> 8); // High byte of content length
            buffer[2] = (byte)Content.Length; // Low byte of content length
            Array.Copy(Content, 0, buffer, 3, Content.Length); // Content

            @out.Write(buffer, 0, buffer.Length); // Write buffer to stream
        }
    }
}