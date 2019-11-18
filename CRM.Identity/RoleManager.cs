using CRM.DAL;
using CRM.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.Identity
{
    public class RoleManager : RoleManager<Role>
    {
        public RoleManager(IRoleStore<Role, string> roleStore)
       : base(roleStore)
        {
        }

        public static RoleManager Create(IdentityFactoryOptions<RoleManager> options, IOwinContext context)
        {
            var roleManager = new RoleManager(new RoleStore<Role>(context.Get<CRMContext>()));

            return roleManager;
        }
    }
}
