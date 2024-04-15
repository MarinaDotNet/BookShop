using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookShop.API.Models.Authentication
{
    public class AuthenticationApiDbContext(DbContextOptions<AuthenticationApiDbContext> options) : IdentityDbContext<ApiUser, IdentityRole, string>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            string adminId = Guid.NewGuid().ToString();
            string userId = Guid.NewGuid().ToString();
            string roleAdminId = Guid.NewGuid().ToString();
            string roleUserId = Guid.NewGuid().ToString();

            var hasher = new PasswordHasher<ApiUser>();
            builder.Entity<ApiUser>().HasData(
                new ApiUser
                {
                    Id = adminId,
                    Email = "admin@email.com",
                    NormalizedEmail = "admin@email.com",
                    UserName = "admin",
                    NormalizedUserName = "admin",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PasswordHash = hasher.HashPassword(null!, "P@ssw0rd")
                },
                new ApiUser
                {
                    Id = userId,
                    Email = "user@email.com",
                    NormalizedEmail = "user@email.com",
                    UserName = "user",
                    NormalizedUserName = "user",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PasswordHash = hasher.HashPassword(null!, "P@ssw0rd")
                });

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = roleAdminId,
                    Name = "admin",
                    NormalizedName = "admin"
                },
                new IdentityRole
                {
                    Id = roleUserId,
                    Name = "user",
                    NormalizedName = "user"
                });

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = roleAdminId,
                    UserId = adminId
                },
                new IdentityUserRole<string>
                {
                    RoleId = roleUserId,
                    UserId = userId
                });

            base.OnModelCreating(builder);
        }
    }
}
