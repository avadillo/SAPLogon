using SAPTools.LogonTicket;
using SAPTools.LogonTicket.Extensions;
using System.Text;

AssertionTicket t1 = new() {
    User = "DEMOUSER",
    SysID = "SSO",
    SysClient = "000",
    RcptSysID = "NWA",
    RcptSysClient = "752",
    Language = SAPLanguage.EN  //Optional
};

LogonTicket t2 = new() {
    User = "DEMOUSER",
    SysID = "SSO",
    SysClient = "000",
    PortalUser = "support@saptools.mx",
    Language = SAPLanguage.EN  //Optional
};

Console.WriteLine("Assertion Ticket:");
Console.WriteLine($"MYSAPSSO2={t1.Create()}");

Console.WriteLine("Logon Ticket:");
Console.WriteLine($"MYSAPSSO2={t2.Create()}");

Console.WriteLine();
Console.WriteLine("Verify the result at https://saptools.mx/mysapsso2");
