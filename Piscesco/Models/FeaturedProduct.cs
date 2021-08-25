using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Piscesco.Models
{
    public class FeaturedProduct
    {

        public int FeaturedProductID { get; set; }

        public int StallID { get; set; }

        public int ProductID { get; set; }
    }
}
