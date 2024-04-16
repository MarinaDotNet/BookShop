using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BookShop.API.Models.Authentication
{
    public class DeleteModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Login is required")]
        [Display(Name = "User Name/Login")]
        public string Login { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Confirm Email required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Display(Name = "Confirm Email")]
        [Compare("EmailAddress", ErrorMessage = "Email and Confirmation Email do not match")]
        public string ConfirmEmail { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your Password, Password is required")]
        [DataType(DataType.Password)]
        [PasswordPropertyText]
        public string Password { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Re enter password")]
        [PasswordPropertyText]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirmation password do not match.")]
        [Display(Name = "Confirmation Password")]
        public string ConfirmPassword { get; set;} = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please confirm or not the deletion process")]
        [Display(Name = "Are you sure to delete account?")]
        public bool ConfirmDelete { get; set; }

    }
}
