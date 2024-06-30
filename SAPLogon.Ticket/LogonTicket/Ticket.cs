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
    protected List<InfoUnit> InfoUnits = new();

    public string mTicket = string.Empty;
    protected byte[]? mSignature;
    protected byte[]? rawTicket;
    private static readonly byte[] NO_CERTS = {};

    public string Create() {
        encoding = codepage switch {
            "4110" => Encoding.UTF8,
            "4103" => Encoding.Unicode,
            _ => Encoding.ASCII,
        };

        MemoryStream ticketStream = new(256);
        ticketStream.WriteByte(2);
        ticketStream.WriteByte((byte)codepage.ToArray()[0]);
        ticketStream.WriteByte((byte)codepage.ToArray()[1]);
        ticketStream.WriteByte((byte)codepage.ToArray()[2]);
        ticketStream.WriteByte((byte)codepage.ToArray()[3]);

        EncodeInfoUnits();
        foreach (InfoUnit iu in InfoUnits) iu.WriteTo(ticketStream);
        ticketStream.Flush();

        mSignature = SignTicket(ticketStream.ToArray());
        InfoUnit iuSignature = new(InfoUnit.ID_SIGNATURE, mSignature);
        iuSignature.WriteTo(ticketStream);
        ticketStream.Flush();

        rawTicket = ticketStream.ToArray();
        return SAPTools.Utils.Base64.Encode(rawTicket);
    }

    protected byte[] SignTicket(byte[] data) {
        X509Store store = new();
        store.Open(OpenFlags.ReadOnly);
        X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindBySubjectName, SysID, false);
            
        int index = -1;
        for (int i=0; i<col.Count;i++) { if (col[i].FriendlyName == SysID) { index = i; break; } }
        if (index < 0) return NO_CERTS; 

        X509Certificate2 cert = col[index];
        ContentInfo content = new(new Oid("1.2.840.113549.1.7.1"),data);
        SignedCms signedCms = new(content,true);
        _ = new CmsRecipient(cert);
        CmsSigner signer;
        AsymmetricAlgorithm? PK = cert.GetDSAPrivateKey();
        signer = PK != null
            ? new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert, PK) {
                DigestAlgorithm = new Oid("1.3.14.3.2.26")
            }
            : new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert, cert.GetRSAPrivateKey());

        _ = signer.SignedAttributes.Add(new Pkcs9SigningTime(DateTime.UtcNow));            
        signedCms.ComputeSignature(signer);
        if (! IncludeCert && signer.Certificate is not null) signedCms.RemoveCertificate(signer.Certificate);

        return signedCms.Encode();
    }

    protected internal void EncodeInfoUnits() {
        MemoryStream baos = new();
        _ = new StreamWriter(baos, encoding);

        if ( (ValidTime!=null?ValidTime:0) *60 + ValidTimeMin == 0) ValidTimeMin = 2; // Assertion Tickets have 2 mins validity

        if (PortalUser != null) 
            InfoUnits.Add(new InfoUnit(InfoUnit.ID_PORTAL_USER, 
                encoding.GetBytes("portal:"+PortalUser)));
        InfoUnits.Add(new InfoUnit(InfoUnit.ID_AUTHSCHEME, 
            encoding.GetBytes("default")));
        InfoUnits.Add(new InfoUnit(InfoUnit.ID_USER, 
            encoding.GetBytes(User)));
        InfoUnits.Add(new InfoUnit(InfoUnit.ID_CREATE_CLIENT, 
            encoding.GetBytes(SysClient)));
        InfoUnits.Add(new InfoUnit(InfoUnit.ID_CREATE_NAME, 
            encoding.GetBytes(SysID)));
        if (Language != null) 
            InfoUnits.Add(new InfoUnit(InfoUnit.ID_LANGUAGE, 
                encoding.GetBytes(Language)));
        // Assertion Tickets
        if(RcptSysID != null && RcptSysClient != null) {
            InfoUnits.Add(new InfoUnit(InfoUnit.ID_RECIPIENT_CLIENT,
                Encoding.ASCII.GetBytes(RcptSysClient)));
            InfoUnits.Add(new InfoUnit(InfoUnit.ID_RECIPIENT_SID,
                Encoding.ASCII.GetBytes(RcptSysID)));
            // Do not cache the ticket
            InfoUnits.Add(new InfoUnit(InfoUnit.ID_FLAGS, [1]));
        }
        InfoUnits.Add(new InfoUnit(InfoUnit.ID_CREATE_TIME, 
            encoding.GetBytes(System.DateTime.UtcNow.ToString("yyyyMMddHHmm"))));
        InfoUnits.Add(new InfoUnit(InfoUnit.ID_VALID_TIME_MIN,
            InfoUnit.Int32ToBytes(ValidTimeMin)));
        if(ValidTime != null)
            InfoUnits.Add(new InfoUnit(InfoUnit.ID_VALID_TIME,
                InfoUnit.Int32ToBytes(ValidTime ?? 0)));
        InfoUnits.Add(new InfoUnit(InfoUnit.ID_USER_UTF,
            Encoding.UTF8.GetBytes(User)));
    }

    public async Task<string> asyncCreate() =>
        await Task.Run(() => Create());
}
