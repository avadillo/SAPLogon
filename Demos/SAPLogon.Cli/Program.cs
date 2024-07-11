using SAPTools.LogonTicket;
using SAPTools.LogonTicket.Extensions;
using System.Security.Cryptography.X509Certificates;

UserCertificates.PrintCertificates();
Console.WriteLine(UserCertificates.GetWebsiteLoadCertificates());

string thumb = UserCertificates.GetThumbprintBySubject("OU=SAP Tools, CN=SAP SSO RSA 4096");
var (sysId, sysClient) = UserCertificates.GetTypeAndPosition(thumb);
AssertionTicket t1 = new() {
    User = "DEMOUSER",
    SysID = sysId,
    SysClient = sysClient,
    RcptSysID = "NWA",
    RcptSysClient = "752",
    Language = SAPLanguage.EN,  //Optional
    CertificateThumbprint = thumb
};

thumb = UserCertificates.GetThumbprintBySubject("OU=SAP Tools, CN=SAP SSO RSA 2048");
(sysId, sysClient) = UserCertificates.GetTypeAndPosition(thumb);
LogonTicket t2 = new() {
    User = "DEMOUSER",
    SysID = sysId,
    SysClient = sysClient,
    PortalUser = "support@saptools.mx",
    Language = SAPLanguage.EN,  //Optional
    CertificateThumbprint = UserCertificates.GetThumbprintBySubject("OU=SAP Tools, CN=SAP SSO RSA 4096") ?? throw new Exception("Certificate not found")
};

Console.WriteLine("Assertion Ticket:");
Console.WriteLine($"MYSAPSSO2={t1.Create()}");

Console.WriteLine("Logon Ticket:");
Console.WriteLine($"MYSAPSSO2={t2.Create()}");

Console.WriteLine();
Console.WriteLine("Verify the result at https://saptools.mx/mysapsso2");