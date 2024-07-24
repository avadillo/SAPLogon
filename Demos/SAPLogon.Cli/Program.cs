using SAPTools.Ticket;
using SAPTools.Ticket.Extensions;
using SAPLogon.Web.Common;
using System.Diagnostics;

string subject = "OU=SAP Tools, CN=SAP SSO ECDSA P-256";
AssertionTicket t = new() {
    User = "DEMO",
    SysID = "ECDSA",
    SysClient = "000",
    Subject = subject,
    RcptSysID = "NWA",
    RcptSysClient = "752",
    Language = SAPLanguage.ES
};
string ticket = t.Create();
Console.WriteLine($@"Assertion Ticket for ""{t.User}"" issued by ""{t.SysID}""");
Console.WriteLine(ticket);
Console.WriteLine();
Ticket t1 = Ticket.ParseTicket(ticket);
Console.WriteLine("Ticket parsed:");
Console.WriteLine($@"User: {t1.GetValue(InfoUnitID.User)}");
Console.WriteLine($@"Issuing System ID: {t1.GetValue(InfoUnitID.CreateSID)}");
Console.WriteLine($@"Issuing System Client: {t1.GetValue(InfoUnitID.CreateClient)}");
Console.WriteLine($@"Creation Time: {t1.GetValue(InfoUnitID.CreateTime)}");
Console.WriteLine($@"Valid Time (H): {t1.GetValue(InfoUnitID.ValidTimeInH)}");
Console.WriteLine($@"Valid Time (M): {t1.GetValue(InfoUnitID.ValidTimeInM)}");
Console.WriteLine($@"Is RFC: {t1.GetValue(InfoUnitID.RFC)}");
Console.WriteLine($@"Flags: {t1.GetValue(InfoUnitID.Flags)}");
Console.WriteLine($@"Language: {t1.GetValue(InfoUnitID.Language)}");
Console.WriteLine($@"User (UTF8): {t1.GetValue(InfoUnitID.UTF8_User)}");
Console.WriteLine($@"Issuing System ID (UTF8): {t1.GetValue(InfoUnitID.UTF8_CreateSID)}");
Console.WriteLine($@"Issuing System Client (UTF8): {t1.GetValue(InfoUnitID.UTF8_CreateClient)}");
Console.WriteLine($@"Creation Time (UTF8): {t1.GetValue(InfoUnitID.UTF8_CreateTime)}");
Console.WriteLine($@"Language (UTF8): {t1.GetValue(InfoUnitID.UTF8_Language)}");
Console.WriteLine($@"Subject: {t1.Subject}");
Console.WriteLine();

Console.WriteLine($@"Getting certificate by subject  ""{subject}""");
var cert = UserCertificates.GetCertificateBySubject(subject).Result;
if(cert == null) {
    Console.WriteLine("Certificate not found");
    return;
}
Console.WriteLine($"Certificate Thumpbrint: {cert.Thumbprint}");
Console.WriteLine();
Console.WriteLine("Getting the list of valid users for the WebGUI Demo:");
foreach(var a in Catalogs.WebGUIUsers.Result)
    Console.WriteLine($"{a.FullName} ({a.User})");

Stopwatch sw = new();
sw.Start();
Console.WriteLine("\nLanguages with description in English");
Catalogs.ResetInstalledLanguages();
Catalogs.Language = SAPLanguage.EN;
foreach(var a in Catalogs.InstalledLanguages.Result)
    Console.WriteLine($"{a.SAPCode} - {a.ISOCode} - {a.Name}");
sw.Stop();
Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");

sw.Restart();
Console.WriteLine("\nLanguages with description in German");
Catalogs.ResetInstalledLanguages();
Catalogs.Language = SAPLanguage.DE;
foreach(var a in Catalogs.InstalledLanguages.Result)
    Console.WriteLine($"{a.SAPCode} - {a.ISOCode} - {a.Name}");
sw.Stop();
Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");

sw.Restart();
Console.WriteLine("\nLanguages with description in Spanish");
Catalogs.ResetInstalledLanguages();
Catalogs.Language = SAPLanguage.ES;
foreach(var a in Catalogs.InstalledLanguages.Result)
    Console.WriteLine($"{a.SAPCode} - {a.ISOCode} - {a.Name}");
sw.Stop();
Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");


Console.WriteLine(UserCertificates.GetWebsiteLoadCertificates().Result);  