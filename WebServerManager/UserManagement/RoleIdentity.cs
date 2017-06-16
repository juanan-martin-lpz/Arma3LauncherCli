using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ServerWebManager.UserManagement
{
    public class RoleIdentity : IdentityRole
    {
        public RoleIdentity() : base() { }
        public RoleIdentity(string name) : base(name) { }
    }
}