using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAPLogon.Web.Common;
using SAPTools.LogonTicket;
using SAPTools.LogonTicket.Extensions;

namespace SAPLogon.Web.Pages;

public class WebGuiModel : PageModel {
    [BindProperty]
    public string? Cert { get; set; }
    [BindProperty]
    public string? UserName { get; set; }
    [BindProperty]
    public string? Language {
        get => _language.ToString();
        set => _language = value != null ? Enum.Parse<SAPLanguage>(value):null;
    }
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
            .ContinueWith(task => task.Result.Select(cert => new SelectListItem { Text = cert.FriendlyName, Value = cert.Thumbprint }).ToList());

        Task<List<SelectListItem>> usersTask = WebServices.WebGUIUsers
            .ContinueWith(task => task.Result.Select(user => new SelectListItem { Text = user.FullName, Value = user.User }).ToList());

        Task<List<SelectListItem>> languagesTask = WebServices.InstalledLanguages
            .ContinueWith(task => task.Result.Select(lang => new SelectListItem { Text = $"{lang.Name} ({lang.ISOCode})", Value = lang.ISOCode }).ToList());

        // Wait for both operations to complete
        await Task.WhenAll(certificatesTask, usersTask, languagesTask);

        // Retrieve the results
        CertList = await certificatesTask;
        UserList = await usersTask;
        LangList = await languagesTask;

        if(CertList.Count > 0) Cert = CertList[CertList.Count - 1].Value;
        if(UserList.Count > 0) UserName = UserList[0].Value;
    }

    public async Task OnPostSubmit() {
        string domain = GetDomainFromHost(HttpContext.Request.Host.Value);

        if(String.IsNullOrWhiteSpace(Cert)) {
            TxtStatus = "Please select a valid certificate";
            return;
        }

        if(String.IsNullOrWhiteSpace(UserName)) {
            TxtStatus = "Please select a valid user";
            return;
        }

        // Start both tasks without awaiting them immediately to potentially run them in parallel
        var typeAndPositionTask = UserCertificates.GetTypeAndPosition(Cert);
        var certificateTask = UserCertificates.GetCertificate(Cert);

        // Now await the tasks
        var (sysId, sysClient) = await typeAndPositionTask;
        var certificate = await certificateTask;

        LogonTicket ticket = new() {
            SysID = sysId,
            SysClient = sysClient,
            User = UserName,
            ValidTime = 10,
            Certificate = certificate
        };
        if (_language != null) ticket.Language = _language.Value;

        CookieOptions cookieOptions = new() {
            Path = "/",
            Secure = true,
            Domain = $".{domain}",
            SameSite = SameSiteMode.Lax
        };

        // Delete the cookies before setting a new one:
        DeleteCookie("MYSAPSSO2");
        DeleteCookie("SAP_SESSIONID_NWA_752");
        Response.Cookies.Append("MYSAPSSO2", ticket.Create(), cookieOptions);

        // Once the cookie is set, redirect to the SAP system:
        string url = $"https://sapnwa.{domain}/sap/bc/gui/sap/its/webgui";
        Response.Redirect(url);
    }

    private void DeleteCookie(string cookieName) {
        try { Response.Cookies.Delete(cookieName); } catch { /* Log or handle the error if necessary */ }
    }

    private static string GetDomainFromHost(string hostValue) {
        string[] values = hostValue.Split('.');
        return values.Length >= 2 ? $"{values[^2]}.{values[^1]}" : "saptools.mx";
    }
}

