using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAPTools.LogonTicket;
using SAPTools.LogonTicket.Extensions;
using System.Security.Cryptography.X509Certificates;

namespace SAPLogon.Web.Pages;

public class WebGuiModel : PageModel {
    [BindProperty]
    public string? Cert { get; set; }
    public string TxtStatus { get; set; } = "";
    public List<SelectListItem>? CertList { get; private set; } = [];

    public WebGuiModel() => InitializeCertificates();

    private void InitializeCertificates() =>
        CertList?.AddRange(UserCertificates.Certificates.Select(cert => new SelectListItem { Text = cert.FriendlyName, Value = cert.Thumbprint }));

    public void OnPostSubmit() {
        string domain = GetDomainFromHost(HttpContext.Request.Host.Value);

        if (String.IsNullOrWhiteSpace(Cert)) {
            TxtStatus = "Please select a valid certificate";
            return;
        }

        var (sysId, sysClient) = UserCertificates.GetTypeAndPosition(Cert);
        LogonTicket ticket = new() {
            SysID = sysId,
            SysClient = sysClient,
            User = "DEMOUSER",
            ValidTime = 10,
            Language = SAPLanguage.EN,
            CertificateThumbprint = Cert
        };

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
        string url = $"https://sapnwa.{domain}/sap/bc/gui/sap/its/webgui?~transaction=STRUSTSSO2";
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

