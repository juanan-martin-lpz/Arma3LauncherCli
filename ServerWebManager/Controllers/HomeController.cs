using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using ServerWebManager.UserManagement;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;
using Microsoft.Owin.Host.SystemWeb;

namespace ServerWebManager.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SignIn(string usr, string password)
        {
            IAuthenticationManager authMgr = HttpContext.GetOwinContext().Authentication;
            WebUserManager userMrg = HttpContext.GetOwinContext().GetUserManager<WebUserManager>();
            UserIdentity user = await userMrg.FindAsync(usr, password);

            authMgr.SignIn(await userMrg.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));

            return RedirectToAction("Index","Dashboard");
        }

        public ActionResult SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}