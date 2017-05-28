using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Cookies;
using ServerWebManager.UserManagement;
using System;
using Microsoft.Owin.Security.OAuth;

[assembly: OwinStartup(typeof(ServerWebManager.IdentityConfig))]

namespace ServerWebManager
{
    public class IdentityConfig
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext<WebContextIdentity>(WebContextIdentity.Create);

            app.CreatePerOwinContext<WebUserManager>(WebUserManager.Create);
            app.CreatePerOwinContext<WebRoleManager>(WebRoleManager.Create);

            //app.UseCookieAuthentication(new CookieAuthenticationOptions { AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie });

            app.UseOAuthBearerTokens(new OAuthAuthorizationServerOptions { Provider = new WebAuthProvider(), AllowInsecureHttp = true, TokenEndpointPath = new PathString("/Authenticate") });
        }
    }
}