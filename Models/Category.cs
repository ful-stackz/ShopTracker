using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTracker.Models
{
    public class Category
    {
        // Keys
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryID { get; set; } // Primary key

        // Properties
        [MinLength(3)]
        [MaxLength(25)]
        public string Name { get; set; }

        // Navigators
        public List<Item> Items { get; set; }
    }
}
