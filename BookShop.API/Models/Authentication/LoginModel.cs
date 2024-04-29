using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookShop.API.Models.Authentication
{
    public class LoginModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, enter your email")]
        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Please, enter valid email address")]
        public string UserEmail { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, enter your password")]
        [PasswordPropertyText(true)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty!;
    }
}
