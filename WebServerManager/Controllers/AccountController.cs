using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;
using Microsoft.Owin.Host.SystemWeb;
using ServerWebManager.UserManagement;

namespace ServerWebManager.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            IAuthenticationManager authMgr = HttpContext.GetOwinContext().Authentication;
            WebUserManager userMrg = HttpContext.GetOwinContext().GetUserManager<WebUserManager>();
            UserIdentity user = await userMrg.FindAsync(model.username, model.password);

            authMgr.SignIn(await userMrg.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));

           

            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}