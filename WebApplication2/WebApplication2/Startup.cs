using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Owin;

[assembly: OwinStartup(typeof(WebApplication2.Startup))]

namespace WebApplication2
{
    public class Startup
    {
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static readonly string ClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static readonly string Authority = ConfigurationManager.AppSettings["ida:Authority"];
        private static readonly string RedirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static readonly string PostLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Authority = Authority,
                RedirectUri = RedirectUri,
                CallbackPath = new PathString("/signin-oidc"),
                PostLogoutRedirectUri = PostLogoutRedirectUri,
                ResponseType = "code",
                Scope = "openid profile email",
                UsePkce = false,

                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false
                },

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    // The Katana middleware's built-in authorization-code redemption
                    // fails silently for personal Microsoft accounts (MSA) via the
                    // /common endpoint. Redeem the code manually, build the identity
                    // from the returned id_token, and sign in via the cookie middleware.
                    AuthorizationCodeReceived = async n =>
                    {
                        var tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
                        var form = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "grant_type", "authorization_code" },
                            { "client_id", ClientId },
                            { "client_secret", ClientSecret },
                            { "code", n.Code },
                            { "redirect_uri", RedirectUri },
                            { "scope", "openid profile email" }
                        });

                        string idToken;
                        using (var http = new HttpClient())
                        {
                            var resp = await http.PostAsync(tokenEndpoint, form);
                            resp.EnsureSuccessStatusCode();
                            var body = await resp.Content.ReadAsStringAsync();
                            idToken = JObject.Parse(body).Value<string>("id_token");
                        }

                        var handler = new JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(idToken);

                        var identity = new System.Security.Claims.ClaimsIdentity(
                            jwt.Claims, CookieAuthenticationDefaults.AuthenticationType);

                        n.OwinContext.Authentication.SignIn(identity);
                        n.Response.Redirect("/");
                        n.HandleResponse();
                    }
                }
            });
            app.UseStageMarker(PipelineStage.Authenticate);
        }
    }
}
