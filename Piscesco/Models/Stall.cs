using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Piscesco.Models
{
    public class Stall
    {
        public int StallID { get; set; }
        public string OwnerID { get; set; }


        [Display(Name = "Stall Name")]
        public string StallName { get; set; }
        public string Description { get; set; }

        [Display(Name = "Stall Image")]
        public string StallImage { get; set; }

    }
}
