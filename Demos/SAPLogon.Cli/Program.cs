using SAPTools.LogonTicket;

Ticket t = new() {
    User = "DEMOUSER",
    SysID = "SSO",
    SysClient = "000",
    ValidTimeMin = 2,
    RcptSysID = "NWA",
    RcptSysClient = "752",
    IncludeCert = false
};

Console.WriteLine("Ticket:");
Console.WriteLine($"MYSAPSSO2={t.Create()}");
Console.WriteLine();
Console.WriteLine("Verify the result at https://saptools.mx/mysapsso2");