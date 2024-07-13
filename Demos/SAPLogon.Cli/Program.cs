using SAPLogon.Web.Common;

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

foreach(var a in WebServices.WebGUIUsers.Result)
    Console.WriteLine($"{a.FullName} ({a.User})");

foreach (var a in WebServices.InstalledLanguages.Result)
    Console.WriteLine($"{a.SAPCode} - {a.ISOCode} - {a.Name}");
