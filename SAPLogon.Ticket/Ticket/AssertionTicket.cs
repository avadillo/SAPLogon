using SAPTools.Ticket.Extensions;

namespace SAPTools.Ticket;

public class AssertionTicket : Ticket {
    /// <summary>
    /// Recipient System ID (SID)
    /// </summary>
    public required string RcptSysID { get; set; }
    /// <summary>
    /// Recipient System Client (MANDT)
    /// </summary>
    public required string RcptSysClient { get; set; }
    /// <summary>
    /// SAP Logon Language
    /// Please use the SAPLanguage enum for this property
    /// </summary>
    public SAPLanguage Language { get; set; } = SAPLanguage.None;

    protected override void EncodeInfoUnits() {
        base.ValidTime = 2;
        base.EncodeInfoUnits();
        base.InfoUnits.Add(new(InfoUnitID.RecipientClient, RcptSysClient));
        base.InfoUnits.Add(new(InfoUnitID.RecipientSID, RcptSysID));
        base.InfoUnits.Add(new(InfoUnitID.Flags, (byte)InfoUnitFlags.DoNotCacheTicket)); // Do not store the ticket in the SAP Logon Ticket Cache
        if(Language != SAPLanguage.None)
            InfoUnits.Add(new(InfoUnitID.Language, Language, InternalEncoding));
    }
}