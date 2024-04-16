using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BookShop.API.Models.Authentication
{
    public class DeleteModel
    {
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
