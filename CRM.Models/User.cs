using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Models
{
    public class User : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? IsSuperAdmin { get; set; }
        public bool? IsEnabled { get; set; }
        public short MonHours { get; set; }
        public short TueHours { get; set; }
        public short WedHours { get; set; }
        public short ThursHours { get; set; }
        public short FriHours { get; set; }
        public short SatHours { get; set; }
        public short SunHours { get; set; }
    }
}
