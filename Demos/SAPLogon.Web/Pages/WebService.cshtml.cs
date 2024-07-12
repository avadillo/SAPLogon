using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAPLogon.Web.Common;
using SAPTools.LogonTicket;
using SAPTools.LogonTicket.Extensions;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SAPLogon.Web.Pages;

public class WSModel : PageModel {
    [BindProperty]
    public string? Cert { get; set; }
    [BindProperty]
    public string? UserName { get; set; }
    [BindProperty]
    public bool ShowRequest{ get; set; } = false;
    [BindProperty]
    public bool ShowResponseHeaders { get; set; } = false;
    [BindProperty]
    public bool ParseResponse{ get; set; } = false;

    public string TxtStatus { get; set; } = "";
    public List<SelectListItem>? CertList { get; private set; } = [];
    public List<SelectListItem>? UserList { get; private set; } = [];


    public WSModel() => InitializeLists().Wait();

    public async Task InitializeLists() {
        // Start both asynchronous operations
        Task<List<SelectListItem>> certificatesTask = UserCertificates.Certificates
            .ContinueWith(task => task.Result.Select(cert => new SelectListItem { Text = cert.FriendlyName, Value = cert.Thumbprint }).ToList());

        Task<List<SelectListItem>> usersTask = WebServices.WebServiceUsers
            .ContinueWith(task => task.Result.Select(user => new SelectListItem { Text = user.NameText, Value = user.Bname }).ToList());

        // Wait for both operations to complete
        await Task.WhenAll(certificatesTask, usersTask);

        // Retrieve the results
        CertList = await certificatesTask;
        UserList = await usersTask;
    }

    public async Task<IActionResult> OnPostSubmit() {
        if(String.IsNullOrEmpty(Cert)) {
            TxtStatus = "Please select a valid certificate";
            return Page();
        }

        if(String.IsNullOrEmpty(UserName)) {
            TxtStatus = "Please select a valid user";
            return Page();
        }

        // Start both tasks without awaiting them immediately to potentially run them in parallel
        var typeAndPositionTask = UserCertificates.GetTypeAndPosition(Cert);
        var certificateTask = UserCertificates.GetCertificate(Cert);

        // Now await the tasks
        var (sysId, sysClient) = await typeAndPositionTask;
        var certificate = await certificateTask;

        AssertionTicket ticket = new() {
            SysID = sysId,
            SysClient = sysClient,
            User = UserName,
            Language = SAPLanguage.EN,
            RcptSysID = "NWA",
            RcptSysClient = "752",
            Certificate = certificate
        };

        // Call the SAP Web Service and wait for the response
        Uri uri = new(@"https://sapnwa.saptools.mx/sap/bc/srt/rfc/sap/cat_ping/752/zcatping/test");
        TxtStatus = await GetInfoAsync(uri, ticket.Create());

        return Page();
    }


    public async Task<string> GetInfoAsync(Uri uri, string mysapsso2) {
        StringBuilder sb = new();
        using(HttpClient client = new()) {
            client.DefaultRequestHeaders.Add("SOAPAction", "\"urn:sap-com:document:sap:rfc:functions:CAT_PING:CAT_PINGRequest\"");
            client.DefaultRequestHeaders.Add("MYSAPSSO2", mysapsso2);

            using(HttpContent content = new StringContent(SoapPayload, Encoding.UTF8, "text/xml")) {
                if(ShowRequest)
                    sb.AppendLine($"Calling {uri}")
                      .AppendLine("\nRequest Headers:")
                      .AppendLine(String.Join("\n", client.DefaultRequestHeaders.Select(header => $"{header.Key}: {String.Join(" ", header.Value)}")))
                      .AppendLine("\nRequest Body:")
                      .AppendLine(SoapPayload);

                // Send the request and wait for the response
                HttpResponseMessage response = await client.PostAsync(uri, content);
                string responseXML = await response.Content.ReadAsStringAsync();

                sb.AppendLine($"HTTP Status Code: {response.StatusCode}").AppendLine();
                if(response.StatusCode != HttpStatusCode.OK) {
                    sb.AppendLine($"Error: {response.ReasonPhrase}");
                    return sb.ToString();
                }

                // Append the response headers and body to the StringBuilder
                if(ShowResponseHeaders) {
                    sb.AppendLine("Response Headers:")
                      .AppendLine(String.Join("\n", response.Headers.Select(header => $"{header.Key}: {String.Join(" ", header.Value)}")))
                      .AppendLine();
                }
                if(!ParseResponse) {
                    sb.AppendLine("Response Body:")
                      .AppendLine(responseXML);
                } else { 
                    // Parse the XML response to get the SYSINFO element
                    XDocument xmlDoc = XDocument.Parse(responseXML);
                    XElement? SysInfo = xmlDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "SYSINFO");                 
                    sb.AppendLine(SysInfo == null ? "No SYSINFO element found" : SysInfo.ToString());
                }
            }
        }
        return sb.ToString();
    }

    private const string SoapPayload = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:rfc:functions"">
   <soapenv:Header/>
   <soapenv:Body>
      <urn:CAT_PING/>
   </soapenv:Body>
</soapenv:Envelope>
";
}