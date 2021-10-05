using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MementoHealth.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class PinUnlockViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "PIN")]
        public string Pin { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        // Provider

        [Required]
        [Display(Name = "Provider Name")]
        public string ProviderName { get; set; }

        [Required]
        [Display(Name = "Provider Address")]
        public string ProviderAddress { get; set; }

        [Required]
        [Display(Name = "Provider Email")]
        [EmailAddress]
        public string ProviderEmail { get; set; }

        [Required]
        [Display(Name = "Provider Phone")]
        [Phone]
        public string ProviderPhone { get; set; }


        // Provider Admin

        [Required]
        [Display(Name = "Admin Full Name")]
        public string AdminFullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Admin Email")]
        public string AdminEmail { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Admin Phone")]
        public string AdminPhone { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Admin Password")]
        public string AdminPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm admin password")]
        [Compare("AdminPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmAdminPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
