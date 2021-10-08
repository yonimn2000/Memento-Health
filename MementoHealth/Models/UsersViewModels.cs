using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MementoHealth.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Phone { get; set; }
        public string LockOutStatus { get; set; }
        public bool LockedOut { get; set; }
        public string Role { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required]
        [DisplayName("Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Required]
        public string Role { get; set; }
    }

    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [DisplayName("Full Name")]
        public string FullName { get; set; }

        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Required]
        public string Role { get; set; }
    }
}