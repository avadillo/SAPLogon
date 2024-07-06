using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Text;

namespace SAPTools.LogonTicket;

public class Ticket {
    public string User { get; set; } = "";
    public string? PortalUser { get; set; }
    public string SysID { get; set; } = "";
    public string SysClient { get; set; } = "000";
    public string? Language { get; set; }

    /* Properties for the receiver system (Assertion Tickets) */ 
    public string? RcptSysID { get; set; } 
    public string? RcptSysClient { get; set; }
        
    public string CreateTime { get; set; } = "000000000000";
    public bool IncludeCert { get; set; } = false;
    public int? ValidTime { get; set; }
    public int ValidTimeMin { get; set; } = 0;

    private readonly string codepage = "4103";
    private Encoding encoding = Encoding.Unicode;
    protected List<InfoUnit> InfoUnits = [];

    public string mTicket = "";
    protected byte[]? mSignature;
    protected byte[]? rawTicket;

    public string Create() {
        encoding = codepage switch {
            "4110" => Encoding.UTF8,
            "4103" => Encoding.Unicode,
            _ => Encoding.ASCII,
        };

        using MemoryStream ticketStream = new(256);
        ticketStream.WriteByte(2);
        ticketStream.Write(Encoding.ASCII.GetBytes(codepage), 0, codepage.Length);
        EncodeInfoUnits();
        foreach(InfoUnit iu in InfoUnits) {
            iu.WriteTo(ticketStream);
        }

        mSignature = SignTicket(ticketStream.ToArray());
        InfoUnit iuSignature = new(InfoUnit.ID_SIGNATURE, mSignature);
        iuSignature.WriteTo(ticketStream);

        rawTicket = ticketStream.ToArray();
        return SAPTools.Utils.Base64.Encode(rawTicket);
    }

    protected byte[] SignTicket(byte[] data) {
        using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        var cert = store.Certificates.Find(X509FindType.FindBySubjectName, SysID, false)
                   .OfType<X509Certificate2>()
                   .FirstOrDefault(c => c.FriendlyName == SysID);

        if(cert == null) return [];

        ContentInfo content = new (new Oid("1.2.840.113549.1.7.1"), data);
        SignedCms signedCms = new (content, true);
        CmsSigner signer = new (SubjectIdentifierType.IssuerAndSerialNumber, cert) {
            DigestAlgorithm = new Oid("1.3.14.3.2.26"),
            IncludeOption = IncludeCert ? X509IncludeOption.EndCertOnly: X509IncludeOption.None,
        };

        _= signer.SignedAttributes.Add(new Pkcs9SigningTime(DateTime.UtcNow));
        signedCms.ComputeSignature(signer);

        return signedCms.Encode();
    }

    protected internal void EncodeInfoUnits() {
        // Ensure minimum validity time for Assertion Tickets
        if((ValidTime ?? 0) * 60 + ValidTimeMin == 0) ValidTimeMin = 2;

        // Simplify adding InfoUnits with a local function
        void AddInfoUnit(byte id, byte[] data) => InfoUnits.Add(new InfoUnit(id, data));

        // Use local function to add InfoUnits
        if(PortalUser != null)
            AddInfoUnit(InfoUnit.ID_PORTAL_USER, encoding.GetBytes($"portal:{PortalUser}"));
        AddInfoUnit(InfoUnit.ID_AUTHSCHEME, encoding.GetBytes("default"));
        AddInfoUnit(InfoUnit.ID_USER, encoding.GetBytes(User));
        AddInfoUnit(InfoUnit.ID_CREATE_CLIENT, encoding.GetBytes(SysClient));
        AddInfoUnit(InfoUnit.ID_CREATE_NAME, encoding.GetBytes(SysID));
        if(Language != null)
            AddInfoUnit(InfoUnit.ID_LANGUAGE, encoding.GetBytes(Language));

        // Assertion Tickets
        if(RcptSysID != null && RcptSysClient != null) {
            AddInfoUnit(InfoUnit.ID_RECIPIENT_CLIENT, Encoding.ASCII.GetBytes(RcptSysClient));
            AddInfoUnit(InfoUnit.ID_RECIPIENT_SID, Encoding.ASCII.GetBytes(RcptSysID));
            // Do not cache the ticket
            AddInfoUnit(InfoUnit.ID_FLAGS, new byte[] { 1 });
        }

        AddInfoUnit(InfoUnit.ID_CREATE_TIME, encoding.GetBytes(DateTime.UtcNow.ToString("yyyyMMddHHmm")));
        AddInfoUnit(InfoUnit.ID_VALID_TIME_MIN, InfoUnit.Int32ToBytes(ValidTimeMin));
        if(ValidTime != null)
            AddInfoUnit(InfoUnit.ID_VALID_TIME, InfoUnit.Int32ToBytes(ValidTime.Value));
        AddInfoUnit(InfoUnit.ID_USER_UTF, Encoding.UTF8.GetBytes(User));
    }
}
