using Microsoft.AspNetCore.Mvc.RazorPages;
using SAPTools.LogonTicket;

namespace SAPLogon.Pages
{
    public class webguiModel : PageModel
    {
        public string Message { get; set; } = string.Empty;
        public void OnGet(string user, string tx)
        {
            if (user.ToUpper() == "SAP*" || user.ToUpper() == "DDIC")
            {
                Message = "User not allowed";
                return;
            }
            string url = @"https://sapnwa.aptus.mx/sap/bc/gui/sap/its/webgui";

            Message = "Redirecting...";

            Ticket t = new() {
                SysID = "SSO-RSA",
                User = (user == "") ? "DEMOUSER":user 
            };

            var cookieOptions = new CookieOptions
            {
                //Expires = DateTime.Now.AddMinutes(5),
                Path = "/",
                Secure = true,
                HttpOnly = true,
                Domain = "aptus.mx",
                SameSite = SameSiteMode.Lax
            };
            var cookieOptions2 = new CookieOptions
            {
                //Expires = DateTime.Now.AddHours(10),
                Path = "/",
                Secure = true,
                //Domain = "sapnwa.aptus.mx",
                SameSite = SameSiteMode.None
            };

            try { Response.Cookies.Delete("MYSAPSSO2", cookieOptions); } catch { }
            try { Response.Cookies.Delete("SAP_SESSIONID_NWA_752", cookieOptions2); } catch { }
            Response.Cookies.Append("MYSAPSSO2", t.Create(), cookieOptions);
            
            if (tx is not null)
                url = url + "?~transaction=" + tx.ToUpper(); 

            Response.Redirect(url);
        }
    }
}
