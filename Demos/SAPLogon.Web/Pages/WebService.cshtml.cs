using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAPTools.LogonTicket;
using SAPTools.LogonTicket.Extensions;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SAPLogon.Web.Pages;

public class WSModel : PageModel {
    [BindProperty]
    public string? SysID { get; set; }
    [BindProperty]
    public string TxtStatus { get; set; } = "";
    public List<SelectListItem>? Certificates { get; private set; }

    public WSModel() => InitializeCertificates();

    private void InitializeCertificates() => Certificates = [
        new SelectListItem { Text = "SSO (DSA 1024)", Value = "SSO" },
        new SelectListItem { Text = "SSO-RSA (RSA 2048)", Value = "SSO-RSA" }
    ];

    public async Task<IActionResult> OnPostSubmit() {
        if (String.IsNullOrEmpty(SysID)) {
            TxtStatus = "Please select a valid certificate";
            return Page();
        }

        AssertionTicket ticket = new() {
            SysID = SysID.ToUpper(),
            SysClient = "000",
            User = "DEMOUSER",
            Language = SAPLanguage.EN,
            RcptSysID = "NWA",
            RcptSysClient = "752"
        };

        // Call the SAP Web Service and wait for the response
        Uri uri = new(@"https://sapnwa.saptools.mx/sap/bc/srt/rfc/sap/cat_ping/752/zcatping/test");
        TxtStatus = await GetInfoAsync(uri, ticket.Create());

        return Page();
    }

    public async Task<string> GetInfoAsync(Uri uri, string mysapsso2) {
        // Create the SOAP envelope using the CAT_PING action
        string soapString = CreateSoapEnvelope("CAT_PING");

        StringBuilder sb = new();
        using(HttpClient client = new()) {
            client.DefaultRequestHeaders.Add("SOAPAction", "\"urn:sap-com:document:sap:rfc:functions:CAT_PING:CAT_PINGRequest\"");
            client.DefaultRequestHeaders.Add("MYSAPSSO2", mysapsso2);

            using(HttpContent content = new StringContent(soapString, Encoding.UTF8, "text/xml")) {
                _ = sb.AppendLine($"Calling {uri}")
                  .AppendLine("\nRequest Headers:")
                  .AppendLine(String.Join("\n", client.DefaultRequestHeaders.Select(header => $"{header.Key}: {String.Join(" ", header.Value)}")))
                  .AppendLine("\nRequest Body:")
                  .AppendLine(soapString);

                // Send the request and wait for the response
                HttpResponseMessage response = await client.PostAsync(uri, content);
                string responseXML = await response.Content.ReadAsStringAsync();

                _ = sb.AppendLine($"\nHTTP Status Code: {response.StatusCode}");
                if(response.StatusCode != HttpStatusCode.OK) {
                    _ = sb.AppendLine($"\nError: {response.ReasonPhrase}");
                    return sb.ToString();
                }

                // Append the response headers and body to the StringBuilder
                _ = sb.AppendLine("\nResponse Headers:")
                      .AppendLine(String.Join("\n", response.Headers.Select(header => $"{header.Key}: {string.Join(" ", header.Value)}")))
                      .AppendLine("\nResponse Body:")
                      .AppendLine(responseXML);

                // Parse the XML response to get the SYSINFO element
                XDocument xmlDoc = XDocument.Parse(responseXML);
                XElement? SysInfo = xmlDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "SYSINFO");
                _ = sb.AppendLine("\nSYSINFO:")
                      .AppendLine(SysInfo == null ? "No SYSINFO element found" : SysInfo.ToString().Replace("><", ">\n<"));
            }
        }
        return sb.ToString();
    }

    private static string CreateSoapEnvelope(string action) => $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:rfc:functions"">
   <soapenv:Header/>
   <soapenv:Body>
      <urn:{action}/>
   </soapenv:Body>
</soapenv:Envelope>
";
}
