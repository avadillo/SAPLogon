using SAPLogon.Web.Common;
using SAPTools.LogonTicket;

string subject = "OU=SAP Tools, CN=SAP SSO RSA 4096";
Console.WriteLine($@"Getting certificate by subject  ""{subject}""");
var cert = UserCertificates.GetCertificateBySubject(subject).Result;
if(cert == null) {
    Console.WriteLine("Certificate not found");
    return;
}
Console.WriteLine($"Certificate Thumpbrint: {cert.Thumbprint}");
Console.WriteLine();
Console.WriteLine("Getting the list of valid users for the WebGUI Demo:");

int i = 0;
foreach(var a in WebServices.WebGUIUsers.Result)
    Console.WriteLine($"{++i:D2}: {a.NameText} ({a.Bname})");
