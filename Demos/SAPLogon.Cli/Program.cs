using SAPLogon.Web.Common;
using System.Diagnostics;

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

Stopwatch sw = new();
sw.Start();
Console.WriteLine("\nLanguages with description in English");
WebServices.ResetInstalledLanguages();
WebServices.Language = SAPTools.LogonTicket.Extensions.SAPLanguage.EN;
foreach(var a in WebServices.InstalledLanguages.Result)
    Console.WriteLine($"{a.SAPCode} - {a.ISOCode} - {a.Name}");
sw.Stop();
Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");

sw.Restart();
Console.WriteLine("\nLanguages with description in German");
WebServices.ResetInstalledLanguages();
WebServices.Language = SAPTools.LogonTicket.Extensions.SAPLanguage.DE;
foreach(var a in WebServices.InstalledLanguages.Result)
    Console.WriteLine($"{a.SAPCode} - {a.ISOCode} - {a.Name}");
sw.Stop();
Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");

sw.Restart();
Console.WriteLine("\nLanguages with description in Spanish");
WebServices.ResetInstalledLanguages();
WebServices.Language = SAPTools.LogonTicket.Extensions.SAPLanguage.ES;
foreach(var a in WebServices.InstalledLanguages.Result)
    Console.WriteLine($"{a.SAPCode} - {a.ISOCode} - {a.Name}");
sw.Stop();
Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
