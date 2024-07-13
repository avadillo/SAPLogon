using SAPTools.LogonTicket.Extensions;

namespace SAPTools.LogonTicket;

public class LogonTicket : Ticket {
    public string? PortalUser { get; set; }
    public override uint ValidTime { get; set; } = 5;

    protected override void EncodeInfoUnits() {
        base.EncodeInfoUnits();
        if(PortalUser != null)
            base.InfoUnits.Add(new(InfoUnitID.PortalUser, $"portal:{PortalUser}"));
        base.InfoUnits.Add(new(InfoUnitID.AuthScheme, "default"));
    }
}