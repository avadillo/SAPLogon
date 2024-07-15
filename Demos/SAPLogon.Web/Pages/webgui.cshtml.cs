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
            .ContinueWith(task => task.Result.Select(cert => new SelectListItem { Text = cert.FriendlyName, Value = cert.Subject }).ToList());

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

        var (sysId, sysClient) = await UserCertificates.GetTypeAndPosition(Cert);
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

        // Once the cookie is set, redirect to the SAP system:
        string url = $"https://demo.saptools.mx/sap/bc/gui/sap/its/webgui?sap-language={_language?.ToString() ?? "EN"}";
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

