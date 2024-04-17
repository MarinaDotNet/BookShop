using System.ComponentModel.DataAnnotations;

namespace BookShop.API.Models.Authentication
{
    public class DeleteAccountModel : DeleteModel
    {
        [Required(ErrorMessage = "Please enter account email to delete account")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Account Email")]
        public string AccountEmail { get; set; } = string.Empty!;

        [Required(ErrorMessage = "Please enter confirm account email to delete account")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Compare("AccountEmail", ErrorMessage = "Account Email and Confirmation Email do not match.")]
        [Display(Name = "Confirm Email")]
        public string AccountEmailConfirm { get; set; } = string.Empty!;
    }
}
