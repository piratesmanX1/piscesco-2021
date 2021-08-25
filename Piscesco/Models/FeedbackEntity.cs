using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Piscesco.Models
{
    public class FeedbackEntity:TableEntity
    {
        // constructor indicating the content specified for the table storage
        [Key]
        public int Id { get; set; }
        public FeedbackEntity(string userID, string orderID)
        {
            this.PartitionKey = userID; // one user can have many orderID
            this.RowKey = orderID;
        }

        public FeedbackEntity() { }

        // content for the table storage
        [Display(Name = "FeedbackDescription")]
        public string FeedbackDescription { get; set; }
        public DateTime FeedbackDate { get; set; }
    }
}
