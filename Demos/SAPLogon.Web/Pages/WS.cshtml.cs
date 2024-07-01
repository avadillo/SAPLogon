using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAPTools.LogonTicket;
using System.Reflection.PortableExecutable;
using System;
using System.Xml;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SAPLogon.Pages;

public class WSModel : PageModel
{
    [BindProperty]
    public string SysID { get; set; } = "";

    [BindProperty]
    public string UserID { get; set; } = "DEMOUSER";

    [BindProperty]
    public string TxtStatus { get; set; } = "";

    public void OnGet() {
        // Nothing to do here
    }

    public async Task<IActionResult> OnPostSubmit() {
        if (SysID is null || SysID == "" || SysID[..3] != "SSO") {
            TxtStatus = "Please select a Valid Certificate";
            return Page();
        }

        Ticket t = new() {
            SysID = SysID.ToUpper(),
            SysClient = "000",
            User = "DEMOUSER",
            ValidTimeMin = 2,
            Language = "E",
            RcptSysID = "NWA",
            RcptSysClient = "752"
        }; 

        Uri uri = new (@"https://sapnwa.aptus.mx/sap/bc/srt/rfc/sap/cat_ping/752/zcatping/test");
        TxtStatus  = await GetInfoAsync(uri, t.Create());

        return Page();
    }

    public async Task<string> GetInfoAsync(Uri uri, string mysapsso2) {
        string soapString = CreateSoapEnvelope(); // Construct your SOAP request here

        HttpClient client = new();
        StringBuilder sb = new();
        client.DefaultRequestHeaders.Add("SOAPAction", @"""urn:sap-com:document:sap:rfc:functions:CAT_PING:CAT_PINGRequest""");
        HttpContent content = new StringContent(soapString, Encoding.UTF8, "text/xml");
        client.DefaultRequestHeaders.Add("MYSAPSSO2", mysapsso2);
        _ = sb.AppendLine ("Calling " + uri);   
        _ = sb.AppendLine("\nRequest Headers:");
        foreach (var header in client.DefaultRequestHeaders) {
            _ = sb.Append(header.Key + ": ");
            foreach (var value in header.Value) {
                _ = sb.Append(value + " ");
            }
            _ = sb.AppendLine();
        }

        _ = sb.AppendLine("\nRequest Body:");
        _ = sb.AppendLine(soapString);

        HttpResponseMessage response = await client.PostAsync(uri, content);
        string responseXML = await response.Content.ReadAsStringAsync();
        _ = sb.AppendLine("\nHTTP Status Code: " + response.StatusCode);
 
        if (response.StatusCode != HttpStatusCode.OK) {
            _ = sb.AppendLine("\nError: " + response.ReasonPhrase);
            return sb.ToString();
        }
        _ = sb.AppendLine("\nResponse Headers:");
        foreach (var header in response.Headers) {
            _ = sb.Append(header.Key + ": ");
            foreach (var value in header.Value) {
                _ = sb.Append(value + " ");
            }
            _ = sb.AppendLine();
        }
        _ = sb.AppendLine("\nResponse Body:");
        _ = sb.AppendLine(responseXML);
        _ = sb.AppendLine("\nSYSINFO:");
        XDocument xmlDoc = XDocument.Parse(responseXML);
        XElement? SysInfo = xmlDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "SYSINFO");

        _ = sb.AppendLine(SysInfo == null ? "No SYSINFO element found" : SysInfo.ToString().Replace("><", ">\n<"));
        return sb.ToString();
    }
    private static string CreateSoapEnvelope() => @"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:rfc:functions"">
   <soapenv:Header/>
   <soapenv:Body>
      <urn:CAT_PING/>
   </soapenv:Body>
</soapenv:Envelope>
";
}
