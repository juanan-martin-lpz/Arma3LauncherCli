using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace ServerManagerCore.Models.Admin
{
    public class UserViewModel
    {
        public int Id {get; set;}
        public string UserName { get; set; }
        public string RoleName { get; set; }             
    }
}
