namespace Pharm.Models
{
    public class EditProfileViewModel
    {
        public string UserName { get; set; } // Read-only field (optional)
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }
}
