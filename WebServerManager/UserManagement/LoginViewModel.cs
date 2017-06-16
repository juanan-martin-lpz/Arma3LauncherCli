using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerWebManager.UserManagement
{
    public class LoginViewModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public bool authenticated { get; set; }
        public string error { get; set; }
        public bool gotError { get; set; }
    }
}