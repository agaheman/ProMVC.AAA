using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProMVC.AAA.WebApplication.Models
{
    public class SiteSettings
    {
        public int NotAllowedPreviouslyUsedPasswords { get; set; }
        public int ChangePasswordReminderDays { get; set; }
        public string[] PasswordsBanList { get; set; }
    }
}
