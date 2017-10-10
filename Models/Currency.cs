using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTracker.Models
{
    public class Currency
    {
        // Keys
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CurrencyID { get; set; } // Primary key

        // Properties
        [MinLength(3)]
        [MaxLength(3)]
        public string Name { get; set; }
        [MaxLength(20)]
        public string FullName { get; set; }

        // Navigation properties
        public List<Purchase> Purchases { get; set; }
        public List<Group> Groups { get; set; }
    }
}
