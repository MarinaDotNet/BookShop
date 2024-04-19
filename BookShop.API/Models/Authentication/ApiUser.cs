using Microsoft.AspNetCore.Identity;

namespace BookShop.API.Models.Authentication
{
    public class ApiUser : IdentityUser
    {
        public virtual ICollection<Order> ? Orders { get; set; } 
    }
}
