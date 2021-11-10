using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MementoHealth.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace MementoHealth.Models
{
    public class IndexViewModel
    {
        public bool HasPin { get; set; }
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePinViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string Password { get; set; }

        [Required]
        [StringLength(ApplicationUserManager.MaxPinLength, ErrorMessage = "The {0} must be between {2} and {1} digits long.", MinimumLength = ApplicationUserManager.MinPinLength)]
        [DataType(DataType.Password)]
        [Display(Name = "New PIN")]
        public string NewPin { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm PIN")]
        [Compare("NewPin", ErrorMessage = "The new PIN and confirmation PIN do not match.")]
        public string ConfirmPin { get; set; }

        public string ReturnUrl { get; set; }
        public bool LockAfterChangingPin { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class ChangeFullNameViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

    public class StatsViewModel
    {
        public int ProviderCount { get; set; }
        public int PatientCount { get; set; }
        public int FormCount { get; set;  }
        public int SubmissionCount { get; set; }
        public int UserCount { get; set; }
        public int AveragePatients { get; set; }
        public int AverageForms { get; set; }
        public int AverageUsers { get; set; }
    }
}