using SAPTools.LogonTicket.Extensions;

namespace SAPTools.LogonTicket;

public class AssertionTicket : Ticket {
    public required string RcptSysID { get; set; }
    public required string RcptSysClient { get; set; }

    public override void EncodeInfoUnits() {
        base.ValidTime = 2;
        base.EncodeInfoUnits();
        base.InfoUnits.Add(new(InfoUnitID.RecipientClient, RcptSysClient));
        base.InfoUnits.Add(new(InfoUnitID.RecipientSID, RcptSysID));
        base.InfoUnits.Add(new(InfoUnitID.Flags, (byte)InfoUnitFlags.DoNotCacheTicket)); // Do not store the ticket in the SAP Logon Ticket Cache
    }
}
