using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTracker.Models
{
    public class Item
    {
        // Keys
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemID { get; set; } // Primary key
        [Required]
        public int CategoryID { get; set; } // Foreign key
        [Required]
        public int MeasureID { get; set; } // Foreign key

        // Properties
        [Required(ErrorMessage = "Can't have an item with no name")]
        [MaxLength(25, ErrorMessage = "Item name can't exceed 25 characters")]
        public string Name { get; set; }
        public string Description { get; set; }
        public int Bought { get; set; }

        // Navigation properties
        public List<Purchase> Purchases { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; }
        [ForeignKey("MeasureID")]
        public Measure Measure { get; set; }

        public Item()
        {
            if (Description == null) Description = "None";
        }
    }
}
