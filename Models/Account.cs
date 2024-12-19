using System.ComponentModel.DataAnnotations;

namespace Pharm.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Password will be hashed
    }
}
