using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTracker.Models
{
    public class User
    {
        // Keys
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; } // Primary key
        public int RoleID { get; set; } // Foreign key

        // Properties
        [Required(ErrorMessage = "You can't have an empty username")]
        [MinLength(4, ErrorMessage = "Your username must be atleast 4 characters long")]
        [StringLength(25, ErrorMessage = "Your username can't exceed 25 characters")]
        public string Username { get; set; }

        // Security
        [Required(ErrorMessage = "You can't have an empty password")]
        [MinLength(6, ErrorMessage = "Your password must be atleast 6 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,}$", 
            ErrorMessage = "Your password must contain atleast one lowercase, one uppercase letter and one number")]
        public string Password { get; set; }
        public string Salt { get; set; }

        // Account related
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy} {1:hh-mm-ss}")]
        public DateTime DateCreated { get; set; }

        // Navigation properties
        [ForeignKey("RoleID")]
        public Role Role { get; set; }
        public List<Group> Groups { get; set; }
        public List<Purchase> Purchases { get; set; }

        // Constructor
        public User()
        {
            // By default the user attains Role = User (RoleID = 3)
            RoleID = 3;

            // Get the DateTime at account creation
            DateCreated = DateTime.Now;
        }

        // Methods
        public void SecurePassword()
        {
            Salt = Security.Protection.MakeSalt(); // Generate new salt (default length = 32)
            Password = Security.Protection.HashPassword(Password, Salt); // Hash the users password using HMACSHA512 and the aboe salt
        }
    }
}
