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
    public class WebUserManager : UserManager<UserIdentity>
    {
        public WebUserManager(IUserStore<UserIdentity> store) : base(store) { }

        public static WebUserManager Create(IdentityFactoryOptions<WebUserManager> options, IOwinContext context)
        {
            WebContextIdentity dbContext = context.Get<WebContextIdentity>();
            WebUserManager manager = new WebUserManager(new UserStore<UserIdentity>(dbContext));
            return manager;
        }
    }
}