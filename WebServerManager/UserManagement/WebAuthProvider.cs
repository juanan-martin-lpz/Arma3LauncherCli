using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace ServerWebManager.UserManagement
{
    public class WebAuthProvider : OAuthAuthorizationServerProvider
    {
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            WebUserManager storeUserMgr = context.OwinContext.Get<WebUserManager>("AspNet.Identity.Owin:" + typeof(WebUserManager).AssemblyQualifiedName);
            UserIdentity user = await storeUserMgr.FindAsync(context.UserName, context.Password);

            if (user == null) { context.SetError("invalid_grant", "Usuario o Password incorrectos"); }
            else
            {

                ClaimsIdentity ident = await storeUserMgr.CreateIdentityAsync(user, "Custom");
                AuthenticationTicket ticket = new AuthenticationTicket(ident, new AuthenticationProperties());
                context.Validated(ticket);
                context.Request.Context.Authentication.SignIn(ident);
            }
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated(); return Task.FromResult<object>(null);
        }
    }
}