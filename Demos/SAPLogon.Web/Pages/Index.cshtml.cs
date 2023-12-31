﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAPTools.LogonTicket;

namespace SAPLogon.Pages
{
    public class IndexModel : PageModel
    {
        public string txtTicket { get; set; } = "";

        [BindProperty]
        public string SysID { get; set; } = "";

        [BindProperty]
        public string UserID { get; set; } = "DEMOUSER";

        public void OnGet() {
            // Nothing to do here
        }

        public void OnPostSubmit() {
            if (SysID == "" ) {
                txtTicket = "Please select a Valid Certificate";
                return;
            }

            if (SysID[..3] != "SSO") return;
            Ticket t = new() {
                SysID = SysID.ToUpper(),
                User = "DEMOUSER", //fixed user for the DEMO
                PortalUser = UserID.ToLower()
            };
            txtTicket = t.Create();

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
            Response.Cookies.Append("MYSAPSSO2", txtTicket, cookieOptions);

            // For a real-life scenario, delete also the cookie "SAP_SESSIONID_SID_CLIENT" generated by the SAP System.
            try { Response.Cookies.Delete("SAP_SESSIONID_NWA_752", sapCookieOptions); } catch { }

            // Once the cookie is set, redirect to the SAP system:
            string url = @"http://sapnwa.aptus.mx/sap/bc/gui/sap/its/webgui";
            Response.Redirect(url);
        }
    }
}
