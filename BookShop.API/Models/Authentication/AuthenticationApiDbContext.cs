using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookShop.API.Models.Authentication
{
    public class AuthenticationApiDbContext(DbContextOptions<AuthenticationApiDbContext> options) : IdentityDbContext<ApiUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
