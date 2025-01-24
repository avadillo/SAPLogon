using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Security.Cryptography.X509Certificates;
using SAPTools.Ticket;

namespace SAPLogon.Web.Pages;

public class FioriModel : PageModel {
    private readonly string[] _forbiddenUsers = ["DDIC", "SAP*"];
    public string Message { get; set; } = "";

    public void OnGet(string? user, string lang) {
        user = user?.ToUpper() ?? "DEMOUSER";

        if(_forbiddenUsers.Any(fu => user.Equals(fu, StringComparison.OrdinalIgnoreCase))) {
            Message = "User not allowed";
            return;
        }

        LogonTicket t = new() {
            SysID = "ECDSA",
            SysClient = "000",
            User = user, PortalUser = "support@saptools.mx",
            Subject = "OU=SAP Tools, CN=SAP SSO ECDSA P-256",
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

        Response.Cookies.Append("MYSAPSSO2", t.Create(), cookieOptions);

        string baseUrl = IsTestEnvironment(HttpContext.Request.Host.Value) ?
            "https://s4.aptus.mx/sap/bc/ui2/flp" :
            "https://s4.aptus.mx/sap/bc/ui2/flp";

        string url = !String.IsNullOrEmpty(lang) ? $"{baseUrl}?sap-language={lang}" : baseUrl;
        Response.Headers["Refresh"] = $"0;url={baseUrl}";
        //Response.Redirect(url);
    }


    private void DeleteCookie(string cookieName) {
        try { Response.Cookies.Delete(cookieName); } catch { /* Log or handle the error if necessary */ }
    }

    private static bool IsTestEnvironment(string hostValue) {
        return hostValue.Contains("localhost") || hostValue.Contains("test", StringComparison.OrdinalIgnoreCase);
    }
}
