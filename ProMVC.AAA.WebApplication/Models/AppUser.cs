using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProMVC.AAA.WebApplication.Models
{
    public class AppUser:IdentityUser
    {
        public AppUser()
        {
            UserUsedPasswords = new HashSet<UserUsedPassword>();
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<UserUsedPassword> UserUsedPasswords { get; set; }
    }
}
