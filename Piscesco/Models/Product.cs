using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Piscesco.Models
{
    public class Product
    {
        [Display(Name = "Product ID")]
        public int ProductID { get; set; }

        public int StallID { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(25, ErrorMessage = "Product name cannot be more than 25 characters.")]
        public string ProductName { get; set; }

        [Display(Name = "Description")]
        [StringLength(100, ErrorMessage = "Description cannot be more than 100 characters.")]
        public string ProductDescription { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Unit")]
        public string ProductUnit { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public int Stock { get; set; }


        [Display(Name = "Product Image")]
        public string ProductImage { get; set; }
    }
}
