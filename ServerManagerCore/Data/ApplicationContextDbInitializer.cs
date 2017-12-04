using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServerManagerCore.Models;
using ServerManagerCore.Models.AccountViewModels;
using ServerManagerCore.Services;
using ServerManagerCore.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ServerManagerCore.Data
{
    public class ApplicationContextDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public ApplicationContextDbInitializer(ApplicationDbContext context)
        {
            _context = context;            
        }

        public ApplicationContextDbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task Seed()
        {

            string roleName = "Administradores";
            string userName = "Admin";
            string password = "Defender100";
            string email = "admin@example.com";

            ApplicationRole adminrole = _context.Roles.FirstOrDefault<ApplicationRole>(a => a.Name == roleName);

            if (adminrole == null)
            {
                adminrole = new ApplicationRole(roleName);

                ApplicationRole editorrole = new ApplicationRole("Editores");
                ApplicationRole memberrole = new ApplicationRole("Miembros");
                ApplicationRole inactiverole = new ApplicationRole("Inactivos");

                _context.Roles.Add(inactiverole);
                _context.Roles.Add(adminrole);
                _context.Roles.Add(editorrole);
                _context.Roles.Add(memberrole);

                ApplicationUser admin = _context.Users.FirstOrDefault<ApplicationUser>(u => u.UserName == userName); //_userManager.FindByNameAsync(userName).Result;

                if (admin == null)
                {
                    PasswordHasher<ApplicationUser> phash = new PasswordHasher<ApplicationUser>();

                    admin = new ApplicationUser { UserName = userName, Email = email };
                    string pwd = phash.HashPassword(admin, password);

                    admin.EmailConfirmed = true;
                    admin.LockoutEnabled = false;
                    admin.NormalizedEmail = "ADMIN@EXAMPLE.COM";
                    admin.NormalizedUserName = "ADMIN";

                    IdentityUserRole<int> userrole = new IdentityUserRole<int>();
                    userrole.RoleId = adminrole.Id;

                    admin.Roles.Add(userrole);

                    admin.PasswordHash = pwd;

                    _context.Users.Add(admin);

                }

                await _userManager.UpdateSecurityStampAsync(admin);


                await _context.SaveChangesAsync();
            }
        }
    }
}