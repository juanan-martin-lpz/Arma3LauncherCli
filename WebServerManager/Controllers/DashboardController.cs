using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServerWebManager.UserManagement;

namespace ServerWebManager.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        [HttpGet]
        [Authorize(Roles = "Administradores")]
        public ActionResult Index()
        {
            return View();
        }
    }
}