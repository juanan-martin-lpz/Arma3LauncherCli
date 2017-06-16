using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace ServerManagerCore.Models
{
    public class WebContextDbInitializer : CreateDatabaseIfNotExists<WebContextIdentity> 
    {
        protected override void Seed(WebContextIdentity context)
        {
            WebUserManager userMgr = new WebUserManager(new UserStore<UserIdentity>(context));
            WebRoleManager roleMgr = new WebRoleManager(new RoleStore<RoleIdentity>(context));

            string roleName = "Administradores";
            string userName = "Admin";
            string password = "secret";
            string email = "admin@example.com";

            if (!roleMgr.RoleExists(roleName)) { roleMgr.Create(new RoleIdentity(roleName)); }
            UserIdentity user = userMgr.FindByName(userName);

            if (user == null) { userMgr.Create(new UserIdentity { UserName = userName, Email = email }, password); user = userMgr.FindByName(userName); }

            if (!userMgr.IsInRole(user.Id, roleName)) { userMgr.AddToRole(user.Id, roleName); }

            base.Seed(context);
        }
    }
}