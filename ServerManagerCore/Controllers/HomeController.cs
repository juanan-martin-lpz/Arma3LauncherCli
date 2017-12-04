using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerManagerCore.Data;

namespace ServerManagerCore.Controllers
{
    public class HomeController : Controller
    {
        Web12BDIDataContext ctx;
        public HomeController(Web12BDIDataContext context)
        {
            ctx = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public async Task<IActionResult> ManageServers()
        {
            ServersController server = new ServersController(ctx);

            return await server.Index();
        }


        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
