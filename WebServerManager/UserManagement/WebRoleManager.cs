using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;


namespace ServerWebManager.UserManagement
{
    public class WebRoleManager : RoleManager<RoleIdentity>
    {
        public WebRoleManager(RoleStore<RoleIdentity> store) : base(store) { }

        public static WebRoleManager Create(IdentityFactoryOptions<WebRoleManager> options, IOwinContext context)
        {
            return new WebRoleManager(new RoleStore<RoleIdentity>(context.Get<WebContextIdentity>()));
        }
    }
}