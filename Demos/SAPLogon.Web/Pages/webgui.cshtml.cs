using Microsoft.AspNetCore.Mvc.RazorPages;
using SAPTools.LogonTicket;

namespace SAPLogon.Pages;

public class WebguiModel : PageModel {
    private readonly string[] _forbiddenUsers = { "DDIC", "SAP*" };
    public string Message { get; set; } = "";

    public void OnGet(string? user, string tx) {
        string domain = GetDomainFromHost(HttpContext.Request.Host.Value);
        user = user?.ToUpper() ?? "DEMOUSER";

        if (_forbiddenUsers.Any(fu => user.Equals(fu, StringComparison.OrdinalIgnoreCase))) {
            Message = "User not allowed";
            return;
        }

        Ticket t = new() { SysID = "SSO-RSA", User = user };
        CookieOptions cookieOptions = new() {
            Path = "/",
            Secure = true,
            Domain = $".{domain}",
            SameSite = SameSiteMode.Lax
        };

        // Delete the cookies before setting a new one:
        DeleteCookie("MYSAPSSO2");
        DeleteCookie("SAP_SESSIONID_NWA_752");
        Response.Cookies.Append("MYSAPSSO2", t.Create(), cookieOptions);

        string url = $"https://sapnwa.{domain}/sap/bc/gui/sap/its/webgui";
        if(!String.IsNullOrEmpty(tx)) url += $"?~transaction={tx}";

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
