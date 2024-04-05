using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookShop.API.Models.Authentication
{
    public class RegisterModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Unique Login is required")]
        public string Login { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Unique Email is required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your Password, Password is required")]
        [DataType(DataType.Password)]
        [PasswordPropertyText]
        public string Password { get; set; } = string.Empty!;
    }
}
