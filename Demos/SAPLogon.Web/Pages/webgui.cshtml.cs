using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAPLogon.Web.Common;
using SAPTools.Ticket;
using SAPTools.Ticket.Extensions;

namespace SAPLogon.Web.Pages;

public class WebGuiModel : PageModel {
    [BindProperty]
    public string? Cert { get; set; }
    [BindProperty]
    public string? UserName { get; set; }
    [BindProperty]
    public string? Language {
        get => _language.ToString();
        set => _language = value != null ? Enum.Parse<SAPLanguage>(value) : null;
    }
    [BindProperty]
    public required string NavigationType { get; set; } = "embedded";
    [BindProperty]
    public string TxtStatus { get; set; } = "";
    public List<SelectListItem>? CertList { get; private set; } = [];
    public List<SelectListItem>? UserList { get; private set; } = [];
    public List<SelectListItem>? LangList { get; private set; } = [];

    private SAPLanguage? _language;
    public WebGuiModel() => InitializeLists().Wait();

    public async Task InitializeLists() {
        // Start both asynchronous operations
        Task<List<SelectListItem>> certificatesTask = UserCertificates.Certificates
            .ContinueWith(task => task.Result.Select(cert => new SelectListItem { Text = cert.Subject, Value = cert.Subject }).ToList());

        Task<List<SelectListItem>> usersTask = Catalogs.WebGUIUsers
            .ContinueWith(task => task.Result.Select(user => new SelectListItem { Text = user.FullName, Value = user.User }).ToList());

        Task<List<SelectListItem>> languagesTask = Catalogs.InstalledLanguages
            .ContinueWith(task => task.Result.Select(lang => new SelectListItem { Text = $"{lang.Name} ({lang.ISOCode})", Value = lang.ISOCode }).ToList());

        // Wait for both operations to complete
        await Task.WhenAll(certificatesTask, usersTask, languagesTask);

        // Retrieve the results
        CertList = await certificatesTask;
        UserList = await usersTask;
        LangList = await languagesTask;

        Cert = "OU=SAP Tools, CN=SAP SSO ECDSA P-256";
        if(UserList.Count > 0) UserName = UserList[0].Value;
    }

    public async Task OnPostSubmit() {
        if(String.IsNullOrWhiteSpace(Cert)) {
            TxtStatus = "Please select a valid certificate";
            return;
        }

        if(String.IsNullOrWhiteSpace(UserName)) {
            TxtStatus = "Please select a valid user";
            return;
        }

        var (sysId, sysClient) = await UserCertificates.GetTypeAndPosition(Cert!);
        LogonTicket ticket = new() {
            SysID = sysId,
            SysClient = sysClient,
            User = UserName,
            ValidTime = 10,
            Subject = Cert,
        };

        CookieOptions cookieOptions = new() {
            Path = "/",
            Secure = true,
            Domain = $".saptools.mx",
            SameSite = SameSiteMode.Lax
        };

        // Delete the cookies before setting a new one:
        DeleteCookie("MYSAPSSO2");
        DeleteCookie("SAP_SESSIONID_NWA_752");
        Response.Cookies.Append("MYSAPSSO2", ticket.Create(), cookieOptions);
    }

    public string GetIframeUrl() {
        string baseUrl = IsTestEnvironment(HttpContext.Request.Host.Value) ?
            "https://demo-test.saptools.mx/webgui-demo" :
            $"https://{GetHostFromReferer(Request.Headers.Referer)}/webgui-demo";
        if(_language is not null) baseUrl += $"?sap-language={_language}";
        if(NavigationType == "embedded") return baseUrl;
        Response.Headers["Refresh"] = $"0;url={baseUrl}";
        return "";
    }

    private void DeleteCookie(string cookieName) {
        try { Response.Cookies.Delete(cookieName); } catch { /* Log or handle the error if necessary */ }
    }

    private static bool IsTestEnvironment(string hostValue) {
        return hostValue.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            || hostValue.Contains("test", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetHostFromReferer(string? referrerUrl) {
        if(String.IsNullOrEmpty(referrerUrl)) return "demo.saptools.mx";
        Uri uri = new(referrerUrl);
        return uri.Host;
    }
}