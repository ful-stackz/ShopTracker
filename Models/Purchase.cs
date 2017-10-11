using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTracker.Models
{
    public class Purchase
    {
        // Keys
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseID { get; set; } // Primary key
        [Required]
        public int GroupID { get; set; } // Foreign key
        public int? UserID { get; set; } // Foreign key
        [Required]
        public int CurrencyID { get; set; } // Foreign key
        [Required]
        public int ItemID { get; set; } // Foreign key

        // Properties
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string Provider { get; set; }

        // Navigators
        public Item Item { get; set; }
        [ForeignKey("CurrencyID")]
        public Currency Currency { get; set; }
        [ForeignKey("GroupID")]
        public Group Group { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        public Purchase()
        {
            if (Provider == null || Provider == "") Provider = "Unknown";
        }
    }
}
