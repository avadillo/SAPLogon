using SAPTools.LogonTicket.Extensions;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SAPTools.LogonTicket;

public abstract class Ticket {
    public required string User { get; set; }
    public required string SysID { get; set; }
    public required string SysClient { get; set; }
    public virtual uint ValidTime { get; set; } = 0;
    public required X509Certificate2 Certificate { get; set; }

    public SAPLanguage Language { get; set; } = SAPLanguage.None;
    public bool IncludeCert { get; set; } = false;
    public bool IsRFC { get; set; } = false;

    private readonly SAPCodepage Codepage = SAPCodepage.UnicodeLittleUnmarked;
    private Encoding InternalEncoding { get; set; } = Encoding.UTF8;
    protected List<InfoUnit> InfoUnits { get; set; } = [];
    private byte[] TicketContent { get; set; } = [];

    public string ToBase64 => SAPTools.Utils.Base64.Encode(TicketContent);
    public byte[] ToBytes => TicketContent;

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

    public virtual void EncodeInfoUnits() {
        InfoUnits.Clear();
        // The following InfoUnits need an encoding depending on the codepage
        InfoUnits.Add(new(InfoUnitID.User, User, InternalEncoding));
        InfoUnits.Add(new(InfoUnitID.CreateClient, SysClient, InternalEncoding));
        InfoUnits.Add(new(InfoUnitID.CreateSID, SysID, InternalEncoding));
        InfoUnits.Add(new(InfoUnitID.CreateTime, DateTime.UtcNow, InternalEncoding));
        if(Language != SAPLanguage.None)
            InfoUnits.Add(new(InfoUnitID.Language, Language, InternalEncoding));

        // The following InfoUnits do not need an encoding
        InfoUnits.Add(new(InfoUnitID.ValidTimeInM, ValidTime % 60));
        if(ValidTime < 60) InfoUnits.Add(new(InfoUnitID.ValidTimeInH, ValidTime / 60));
        InfoUnits.Add(new(InfoUnitID.UTF8_User, User));
        if(IsRFC) InfoUnits.Add(new(InfoUnitID.RFC, "X"));
    }
}