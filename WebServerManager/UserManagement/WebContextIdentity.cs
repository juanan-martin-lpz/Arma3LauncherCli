using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace ServerWebManager.UserManagement
{
    public class WebContextIdentity : IdentityDbContext<UserIdentity>
    {
        public WebContextIdentity() : base("ServerWebManagerIdentityDb") { Database.SetInitializer<WebContextIdentity>(new WebContextDbInitializer()); }
        public static WebContextIdentity Create() { return new WebContextIdentity(); }
    }
}