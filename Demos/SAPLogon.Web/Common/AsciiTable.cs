using System.Data;
using System.Text;

namespace SAPLogon.Web.Common;

public static class AsciiTable {
    public static StringBuilder CreateTable(string responseXML, string soapXPathQuery, string[] columns) {
        StringBuilder sb = new();
        // Define Unicode box-drawing characters
        string horizontalLine = "─";
        string verticalLine = "│";
        string topLeftCorner = "┌";
        string topRightCorner = "┐";
        string bottomLeftCorner = "└";
        string bottomRightCorner = "┘";
        string intersectionT = "┬";
        string intersectionL = "├";
        string intersectionR = "┤";
        string intersectionB = "┴";
        string intersectionCross = "┼";

        DataTable dt = WebServices.ParseSoapResponse(responseXML, soapXPathQuery, columns);

        // Calculate maximum width for each column
        var columnWidths = columns.Select(col => col.Length).ToArray();
        foreach(DataRow row in dt.Rows) {
            for(int i = 0; i < columns.Length; i++) {
                // Safely handle potential null values
                int length = row[columns[i]]?.ToString()?.Length ?? 0;
                if(length > columnWidths[i]) {
                    columnWidths[i] = length;
                }
            }
        }

        // Helper function to create a line
        string CreateLine(string start, string middle, string end, string line, int[] widths) {
            return start + String.Join(middle, widths.Select(width => new string(horizontalLine[0], width + 2))) + end;
        }

        // Print table header
        sb.AppendLine(CreateLine(topLeftCorner, intersectionT, topRightCorner, horizontalLine, columnWidths));
        sb.Append(verticalLine);
        for(int i = 0; i < columns.Length; i++) {
            sb.Append($" {columns[i].PadRight(columnWidths[i])} {verticalLine}");
        }
        sb.AppendLine();

        // Print rows
        sb.AppendLine(CreateLine(intersectionL, intersectionCross, intersectionR, horizontalLine, columnWidths));
        foreach(DataRow row in dt.Rows) {
            sb.Append(verticalLine);
            for(int i = 0; i < columns.Length; i++) {
                // Safely handle potential null values
                var cell = row[columns[i]]?.ToString()?.PadRight(columnWidths[i]) ?? "".PadRight(columnWidths[i]);
                sb.Append($" {cell} {verticalLine}");
            }
            sb.AppendLine();
        }

        sb.AppendLine(CreateLine(bottomLeftCorner, intersectionB, bottomRightCorner, horizontalLine, columnWidths));
        return sb;
    }

}
