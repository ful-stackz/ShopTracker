using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ShopTracker.Models
{
    public class Group
    {
        // Keys
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupID { get; set; } // Primary key
        public int UserID { get; set; } // Foreign key
        public int? PrefCurrID { get; set; } // Foreign key

        // Properties
        [Required(ErrorMessage = "Please give your group a name")]
        [StringLength(20, ErrorMessage = "Your group name can't exceed 20 characters length")]
        public string Name { get; set; }
        [ForeignKey("PrefCurrID")]
        public Currency PreferredCurrency { get; set; }

        // Navigators
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual List<Purchase> Purchases { get; set; }

        public Group()
        {
            if (PrefCurrID == null) PrefCurrID = 2;
        }
    }
}
