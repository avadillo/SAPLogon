using System.Security.Cryptography.Pkcs;
using System.Text;
using SAPTools.Ticket.Extensions;
using static SAPTools.Ticket.Extensions.InfoUnitExtensions;

namespace SAPTools.Ticket;
public class InfoUnit {
    public InfoUnitID ID { get; set; }
    public byte[] Content { get; set; }

    public InfoUnit(InfoUnitID id, byte[] data) =>
        (ID, Content) = (id, data);

    public InfoUnit(InfoUnitID id, byte data) =>
        (ID, Content) = (id, [data]);

    public InfoUnit(InfoUnitID id, string data) =>
        (ID, Content) = (id, id.DetermineEncoding().GetBytes(data));

    public InfoUnit(InfoUnitID id, string data, Encoding enc) =>
        (ID, Content) = (id, id.DetermineEncoding(enc).GetBytes(data));

    public InfoUnit(InfoUnitID id, SAPLanguage data, Encoding enc) =>
        (ID, Content) = (id, enc.GetBytes(data.ToCode()));

    public InfoUnit(InfoUnitID id, DateTime data, Encoding enc) =>
        (ID, Content) = (id, enc.GetBytes(data.ToString(CreationDateFormat)));

    public InfoUnit(InfoUnitID id, SignedCms data) =>
        (ID, Content) = (id, data.Encode());

    public InfoUnit(InfoUnitID id, uint data) =>
        (ID, Content) = (id, new byte[] {
            (byte)(data >> 24), (byte)(data >> 16),
            (byte)(data >> 8), (byte)data });

    public virtual void WriteTo(Stream @out) {
        // Ensure the content length does not exceed ushort.MaxValue - 3
        if(Content!.Length > UInt16.MaxValue - 3) throw new InvalidOperationException("Content is too large.");

        ushort totalLength = (ushort)(3 + Content.Length); // Total length calculation
        byte[] buffer = new byte[totalLength];

        buffer[0] = (byte)ID!; // ID
        buffer[1] = (byte)(Content.Length >> 8); // High byte of content length
        buffer[2] = (byte)Content.Length; // Low byte of content length
        Array.Copy(Content, 0, buffer, 3, Content.Length); // Content

        @out.Write(buffer, 0, buffer.Length); // Write buffer to stream
    }
}
