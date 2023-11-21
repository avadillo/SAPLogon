using SAPTools.LogonTicket;
using SAPTools.Utils;
using System.Text;

Ticket t = new() {
    PortalUser = "DEMOUSER",
    User = "DEMOUSER",
    SysID = "SSO-RSA",
    SysClient = "000",
    ValidTime = 50000,
    IncludeCert = false
};

string _ticket = t.Create();
Console.WriteLine(Hex.HexDump(Base64.Decode(_ticket), 16));
Console.WriteLine("Ticket:");
Console.WriteLine("MYSAPSSO2=" + _ticket);
Console.WriteLine();
Console.WriteLine("Verify the result at https://saptools.mx/mysapsso2");
