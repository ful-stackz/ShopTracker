using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTracker.Models
{
    public class Role
    {
        // Keys
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }

        // Properties
        public string Name { get; set; }

        // Navigation properties
        public List<User> Users { get; set; }
    }
}
