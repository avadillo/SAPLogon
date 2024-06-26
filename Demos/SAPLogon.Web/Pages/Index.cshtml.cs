﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAPTools.LogonTicket;

namespace SAPLogon.Pages
{
    public class IndexModel : PageModel
    {
        public string TxtStatus { get; set; } = "";

        [BindProperty]
        public string SysID { get; set; } = "";

        [BindProperty]
        public string UserID { get; set; } = "DEMOUSER";

        public void OnGet() {
            // Nothing to do here
        }

        public void OnPostSubmit() {
            if (SysID is null || SysID == "" ) {
                TxtStatus = "Please select a Valid Certificate";
                return;
            }

            if (SysID[..3] != "SSO") return;
            Ticket t = new() {
                SysID = SysID.ToUpper(),
                SysClient = "000",
                User = "DEMOUSER",
                ValidTimeMin = 2,
                Language = "E"
            };

            CookieOptions cookieOptions = new() {
                Path = "/",
                Secure =  true,
                HttpOnly = true,
                Domain = ".aptus.mx",
                SameSite = SameSiteMode.Lax
            };

            CookieOptions sapCookieOptions = new() {
                Path = "/",
                Secure = true,
                Domain = "sapnwa.aptus.mx",
                SameSite = SameSiteMode.Lax
            };

            // Delete the cookie before setting a new one:
            try { Response.Cookies.Delete("MYSAPSSO2", cookieOptions); } catch { }
            Response.Cookies.Append("MYSAPSSO2", t.Create(), cookieOptions);

            // For a real-life scenario, delete also the cookie "SAP_SESSIONID_SID_CLIENT" generated by the SAP System.
            try { Response.Cookies.Delete("SAP_SESSIONID_NWA_752", sapCookieOptions); } catch { }

            // Once the cookie is set, redirect to the SAP system:
            string url = @"https://sapnwa.aptus.mx/sap/bc/gui/sap/its/webgui?~transaction=STRUSTSSO2";
            Response.Redirect(url);
        }
    }
}
