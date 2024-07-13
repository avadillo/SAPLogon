using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAPLogon.Web.Common;
using SAPTools.LogonTicket;
using SAPTools.LogonTicket.Extensions;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace SAPLogon.Web.Pages;

public class WSModel : PageModel {
    [BindProperty]
    public string? Cert { get; set; }
    [BindProperty]
    public string? UserName { get; set; }
    [BindProperty]
    public string? Language {
        get => _language.ToString();
        set => _language = value != null ? Enum.Parse<SAPLanguage>(value): null;
    }
    [BindProperty]
    public string Service { get; set; } = "Languages";
    [BindProperty]
    public bool ShowRequest { get; set; } = false;
    [BindProperty]
    public bool ShowResponseHeaders { get; set; } = false;
    [BindProperty]
    public bool ParseResponse { get; set; } = true;

    public string TxtStatus { get; set; } = "";
    public List<SelectListItem>? CertList { get; private set; } = [];
    public List<SelectListItem>? UserList { get; private set; } = [];
    public List<SelectListItem>? LangList { get; private set; } = [];
    public List<SelectListItem>? ServiceList { get; private set; } = [];

    public WSModel() => InitializeLists().Wait();

    private SAPLanguage? _language;
    public async Task InitializeLists() {
        // Start both asynchronous operations
        Task<List<SelectListItem>> certificatesTask = UserCertificates.Certificates
            .ContinueWith(task => task.Result.Select(cert => new SelectListItem { Text = cert.FriendlyName, Value = cert.Thumbprint }).ToList());

        Task<List<SelectListItem>> usersTask = WebServices.WebServiceUsers
            .ContinueWith(task => task.Result.Select(user => new SelectListItem { Text = user.FullName, Value = user.User }).ToList());

        Task<List<SelectListItem>> languagesTask = WebServices.InstalledLanguages
            .ContinueWith(task => task.Result.Select(lang => new SelectListItem { Text = $"{lang.Name} ({lang.ISOCode})", Value = lang.ISOCode }).ToList());

        // Wait for both operations to complete
        await Task.WhenAll(certificatesTask, usersTask, languagesTask);

        // Retrieve the results
        CertList = await certificatesTask;
        UserList = await usersTask;
        LangList = await languagesTask;
        ServiceList = new() {
            new SelectListItem { Text = "1 - Get System Info", Value = "1" },
            new SelectListItem { Text = "2 - Get Installed Languages", Value = "2" },
            new SelectListItem { Text = "3 - List Available Web GUI Users", Value = "3" },
            new SelectListItem { Text = "4 - List Available WebService Users", Value = "4" }
        };
        if (CertList.Count > 0) Cert = CertList[CertList.Count - 1].Value;
        if (UserList.Count > 0) UserName = UserList[0].Value;
        if(ServiceList.Count > 0) Service = "2";
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

        if(String.IsNullOrEmpty(Service)) {
            TxtStatus = "Please select a valid service";
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
            RcptSysID = "NWA",
            RcptSysClient = "752",
            Certificate = certificate
        };

        if (_language != null) ticket.Language = _language.Value;

        TxtStatus = await ExecuteService(Service, ticket.Create());
        return Page();
    }

    private async  Task<string> ExecuteService(string service, string mysapsso2) {
        // Call the SAP Web Service and wait for the response
        Uri uri = new(@"https://sapnwa.saptools.mx/sap/bc/srt/rfc/sap/zssodemo/752/ssodemo/services");

        var soapAction = @"urn:sap-com:document:sap:soap:functions:mc-style:ZSSODEMO:" + Service switch {
            "1" => "ZGetSystemInfoRequest",
            "2" => "ZGetInstalledLanguagesRequest",
            "3" or "4" => "ZGetUsersByGroupRequest",
            _ => throw new InvalidOperationException("Invalid service")
        };

        var soapXPathQuery = @"//soapenv:Envelope/soapenv:Body/d:" + Service switch {
            "1" => "ZGetSystemInfoResponse/Sysinfo",
            "2" => "ZGetInstalledLanguagesResponse/TLangu/item",
            "3" or "4" => "ZGetUsersByGroupResponse/Names/item",
            _ => throw new InvalidOperationException("Invalid service")
        };

        var columns = Service switch {
            "1" => new[] { "Sysid", "Mandt", "Langu", "Uname", "Sapr", "Host", "Opsys", "Dbsys", "Datum", "Uzeit" },
            "2" => new[] { "Spras", "Laiso", "Sptxt" },
            "3" or "4" => new[] { "Bname", "Class", "NameText" },
            _ => throw new InvalidOperationException("Invalid service")
        };

        var soapEnv = Service switch {
            "1" => WebServices.GetSystemInfoEnv(),
            "2" => WebServices.GetInstalledLanguagesEnv(),
            "3" => WebServices.GetUsersByGroupEnv("WEBGUI"),
            "4" => WebServices.GetUsersByGroupEnv("WEBSERVICE"),
            _ => throw new InvalidOperationException("Invalid service")
        };
        return await GetResponseAsync(uri, soapAction, soapEnv, soapXPathQuery, columns, mysapsso2);
    }

    public async Task<string> GetResponseAsync(Uri uri, string soapAction, string soapEnvelope, string soapXPathQuery, string[] columns, string mysapsso2) {
        StringBuilder sb = new();
        HttpClient client = new();

        // Prepare the request
        HttpRequestMessage request = new(HttpMethod.Post, uri);
        request.Headers.Add("SOAPAction", soapAction);
        request.Headers.Add("MYSAPSSO2", mysapsso2);
        request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

        // Log request if needed
        if(ShowRequest) {
            sb.AppendLine($"Calling {uri}")
              .AppendLine("\nRequest Headers:")
              .AppendLine(String.Join("\n", request.Headers.Concat(request.Content.Headers).Select(header => $"{header.Key}: {String.Join(" ", header.Value)}")))
              .AppendLine("\nRequest Body:")
              .AppendLine(soapEnvelope);
        }

        // Send the request and wait for the response
        HttpResponseMessage response = await client.SendAsync(request);
        string responseXML = await response.Content.ReadAsStringAsync();

        sb.AppendLine($"HTTP Status Code: {response.StatusCode}").AppendLine();
        if(response.StatusCode != HttpStatusCode.OK) {
            sb.AppendLine($"Error: {response.ReasonPhrase}");
        }

        // Log response if needed
        if(ShowResponseHeaders) {
            sb.AppendLine("Response Headers:")
              .AppendLine(String.Join("\n", response.Headers.Concat(response.Content.Headers).Select(header => $"{header.Key}: {String.Join(" ", header.Value)}")))
              .AppendLine();
        }

        sb.AppendLine("Response Body:");
        if(ParseResponse) {
            try {
                sb.Append(AsciiTable.CreateTable(responseXML, soapXPathQuery, columns));
            } catch(Exception ex) {
                sb.AppendLine($"Error parsing response: {ex.Message}");
            }
        } else {
            sb.AppendLine(responseXML);
        }


        return sb.ToString();
    }

  }