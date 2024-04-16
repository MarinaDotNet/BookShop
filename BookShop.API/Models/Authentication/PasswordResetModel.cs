using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BookShop.API.Models.Authentication
{
    public class PasswordResetModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Current Password is required")]
        [PasswordPropertyText]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter new password to reset")]
        [PasswordPropertyText]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Re enter new password to reset")]
        [PasswordPropertyText]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [Display(Name = "Confirmation Password")]
        public string ConfirmPassword { get; set; } = string.Empty!;
    }
}
