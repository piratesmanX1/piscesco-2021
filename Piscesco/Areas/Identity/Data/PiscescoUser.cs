using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Piscesco.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the PiscescoUser class
    public class PiscescoUser : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; }

        [PersonalData]
        public string Role { get; set; }

        [PersonalData]
        public string UserAddress { get; set; }


    }
}
