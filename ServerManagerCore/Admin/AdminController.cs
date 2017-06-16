using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerManagerCore.Models;
using ServerManagerCore.Models.AccountViewModels;
using ServerManagerCore.Models.Admin;
using ServerManagerCore.Services;
using ServerManagerCore.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerManagerCore.Admin
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context; 

        public AdminController(  
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = loggerFactory.CreateLogger<AdminController>();
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
           
            IQueryable<UserViewModel> users = from u in _userManager.Users
                        join ur in _roleManager.Roles on u.Roles.First().RoleId equals ur.Id
                        select new UserViewModel() 
                        {Id = u.Id, UserName = u.UserName, RoleName = ur.Name};

            return View(users);
        }


        public IActionResult Edit(int id)
        {
            var user = _userManager.FindByIdAsync(id.ToString()).Result;

            if (user == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            // return View(user);
            return BadRequest();
        }

        public IActionResult SendChangePasswordUrl(int id)
        {
            var user = _userManager.FindByIdAsync(id.ToString()).Result;

            if (user == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            string token =_userManager.GeneratePasswordResetTokenAsync(user).Result;

            //Send email token by email

            return BadRequest();
        }

        public IActionResult EditableIndex()
        {
            IQueryable<UserViewModel> users = from u in _userManager.Users
                                              join ur in _roleManager.Roles on u.Roles.First().RoleId equals ur.Id
                                              select new UserViewModel()
                                              { Id = u.Id, UserName = u.UserName, RoleName = ur.Name };

            ViewBag.roles = _roleManager.Roles;

            //return View(users);

            return BadRequest();
        }
    }
}
