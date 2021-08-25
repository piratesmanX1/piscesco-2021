using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Piscesco.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid OrderID { get; set; }

        public string OwnerID { get; set; }

        [Display(Name = "Stall ID")]
        public int StallID { get; set; }

        [Display(Name = "Product ID")]
        public int ProductID { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [Range(1,int.MaxValue)]
        [Display(Name = "Quantity")]
        public int ProductQuantity { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalPrice { get; set; }
       
        public string Status { get; set; }

        [Display(Name = "Date of Order")]
        public DateTime TransactionDate { get; set; }


    }
}
