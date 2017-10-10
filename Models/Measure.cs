using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTracker.Models
{
    public class Measure
    {
        // Keys
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MeasureID { get; set; } // Primary key

        // Properties
        public string Name { get; set; }

        // Navigation properties
        public List<Item> Items { get; set; }
    }
}
