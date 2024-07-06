using Microsoft.AspNetCore.Mvc.RazorPages;
using SAPTools.LogonTicket;

namespace SAPLogon.Pages
{
    public class webguiModel : PageModel
    {
        public string Message { get; set; } = string.Empty;
        public void OnGet(string? user, string tx)
        {
            user ??= "DEMOUSER";
            if(user.ToUpper() == "SAP*" || user.ToUpper() == "DDIC") {
                Message = "User not allowed";
                return;
            }

            Ticket t = new() {
                SysID = "SSO-RSA",
                User = user
            };

            string domain = "saptools.mx"; // Default domain
            List<string> values = [.. HttpContext.Request.Host.Value.Split('.')];
            if(values.Count >= 2) domain = values.TakeLast(2).ToList().Aggregate((a, b) => a + "." + b);

            var cookieOptions = new CookieOptions {
                //Expires = DateTime.Now.AddMinutes(5),
                Path = "/",
                Secure = true,
                HttpOnly = true,
                Domain = $".{domain}",
                SameSite = SameSiteMode.Lax
            };
            var cookieOptions2 = new CookieOptions {
                //Expires = DateTime.Now.AddHours(10),
                Path = "/",
                Secure = true,
                Domain = $"sapnwa.{domain}",
                SameSite = SameSiteMode.None
            };

            try { Response.Cookies.Delete("MYSAPSSO2", cookieOptions); } catch { }
            try { Response.Cookies.Delete("SAP_SESSIONID_NWA_752", cookieOptions2); } catch { }
            Response.Cookies.Append("MYSAPSSO2", t.Create(), cookieOptions);

            string url = $"https://sapnwa.{domain}/sap/bc/gui/sap/its/webgui";
            if (tx is not null) url += $"?~transacttion={tx.ToUpper()}"; 

            Response.Redirect(url);
        }
    }
}
