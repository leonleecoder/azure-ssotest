using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

namespace WebApplication1
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SignOutBtn_Click(object sender, EventArgs e)
        {
            var ctx = HttpContext.Current.GetOwinContext();
            ctx.Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            ctx.Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = "https://localhost:44365/" },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }
    }
}