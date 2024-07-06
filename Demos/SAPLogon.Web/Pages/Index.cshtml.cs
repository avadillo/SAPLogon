using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAPTools.LogonTicket;

namespace SAPLogon.Pages;

public class IndexModel : PageModel {
    [BindProperty]
    public string? SysID { get; set; }
    public string TxtStatus { get; set; } = "";
    public List<SelectListItem>? Certificates { get; private set; }

    public IndexModel() => InitializeCertificates();

    private void InitializeCertificates() => Certificates = [
        new SelectListItem { Text = "SSO (DSA 1024)", Value = "SSO" },
        new SelectListItem { Text = "SSO-RSA (RSA 2048)", Value = "SSO-RSA" }
    ];

    public void OnPostSubmit() {
        string domain = GetDomainFromHost(HttpContext.Request.Host.Value);

        if (String.IsNullOrWhiteSpace(SysID) || !SysID.StartsWith("SSO")) {
            TxtStatus = "Please select a valid certificate";
            return;
        }

        Ticket ticket = new() {
            SysID = SysID.ToUpper(),
            SysClient = "000",
            User = "DEMOUSER",
            ValidTimeMin = 60,
            ValidTime = 0,     // hours
            Language = "E"
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

