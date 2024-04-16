using Asp.Versioning;
using BookShop.API.Controllers.Services;
using BookShop.API.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace BookShop.API.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("authorization/")]
    public class AuthenticationController(UserManager<ApiUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthenticationController> logger) : ControllerBase
    {
        private readonly UserManager<ApiUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthenticationController> _logger = logger;

        #region of Help Methods
        private void LogingError(Exception error) => _logger.LogError(message: error.Message, args: error.StackTrace);
        private void LogingInformation(string message) => _logger.LogInformation(message: message);
        private void LogingWarning(string message) => _logger.LogWarning(message: message);
        #endregion

        [HttpPost, Route("loging")]
        public async Task<ActionResult> LoginAdmin([FromForm]LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserLogin);
                if(user is not null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    if(!userRoles.Contains(ApiConstants.Admin))
                    {
                        return Warning("Access denied for user: '" + user.UserName + "', current user does not have authority", (int)HttpStatusCode.Unauthorized);
                    }

                    List<Claim> authorizationClaims =
                        [
                        new Claim(ClaimTypes.Name, user.UserName!),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        ];

                    foreach(var role in userRoles)
                    {
                        authorizationClaims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var authorizationSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(1),
                        claims: authorizationClaims,
                        signingCredentials: new SigningCredentials(authorizationSignInKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        exparation = token.ValidTo
                    });
                }

                return Warning(user is null ? "Not registrated user:'" + model.UserLogin + "'." : "Entered incorrect password", (int)HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                LogingError(ex);
                return Problem(ex.Message);
            }
        }

        [HttpPost, Route("register")]
        public async Task<ActionResult> RegisterAdmin([FromForm]RegisterModel model)
        {
            try
            {
                var emailExists = await _userManager.FindByEmailAsync(model.EmailAddress);
                var userNameExists = await _userManager.FindByNameAsync(model.Login);

                if(emailExists is not null || userNameExists is not null)
                {
                    return Warning(emailExists is not null && userNameExists is not null ?
                        "User with this email and login already registrated" :
                        emailExists is not null ?
                        "User with this email already registrated" :
                        "User with this login already registrated");
                }

                ApiUser user = new()
                {
                    Email = model.EmailAddress,
                    UserName = model.Login,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (!result.Succeeded)
                {
                    return Warning("Unable registrate user with entered details: " + user + "at " + DateTime.Now + ". Please check entered details.");
                }

                if(!await _roleManager.RoleExistsAsync(ApiConstants.Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(ApiConstants.Admin));
                }
                if (!await _roleManager.RoleExistsAsync(ApiConstants.User))
                {
                    await _roleManager.CreateAsync(new IdentityRole(ApiConstants.User));
                }

                if(await _roleManager.RoleExistsAsync(ApiConstants.Admin))
                {
                    await _userManager.AddToRoleAsync(user, ApiConstants.Admin);
                }

                string roles = "";
                (await _userManager.GetRolesAsync(user)).ToList().ForEach(x => roles += x + " ");

                LogingInformation("Registrated Successfully:" + user + ", with roles:" + roles + ", at " + DateTime.Now);

                return Ok(new
                {
                    loging = user.UserName,
                    email = user.Email,
                    roles = roles,
                    message = "Registrated Successfully"
                });
            }
            catch(Exception ex)
            {
                LogingError(ex);
                return Problem(ex.Message);
            }
        }

        [HttpPut, Route("password-change")]
        public async Task<ActionResult> PasswordUpdate([FromForm]PasswordChangeModel model)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(model.Email);
                if (userExists is null)
                { 
                    return Warning("User with this email is not registrated", 0);
                }

                if(!await _userManager.CheckPasswordAsync(userExists, model.CurrentPassword))
                {
                    return Warning("Entered Current password is not correct", 0);
                }
                if(!model.NewPassword.Equals(model.ConfirmPassword) || 
                    model.NewPassword.Equals(model.CurrentPassword))
                {
                    return Warning(model.NewPassword.Equals(model.ConfirmPassword) ? 
                        "The new password and confirmation password do not match" :
                        "Entered New Password can not be used, please, try another", 0);
                }

                var result = await _userManager.ChangePasswordAsync(userExists, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    return Warning("Unable to change password for" + userExists.UserName + ", operation declined at" + DateTime.Now + ". Please check entered details.", 0);
                }

                LogingInformation("Password changed Successfully:" + userExists.UserName + ", at " + DateTime.Now);

                return Ok("Password changed Successfully");
            }
            catch (Exception ex)
            {
                LogingError(ex);
                return Problem(ex.Message);
            }
        }

        private ActionResult Warning(string message, int statusCode)
        {
            LogingWarning(message);
            return statusCode == (int)HttpStatusCode.Unauthorized ? 
                Unauthorized(message) :
                Problem(message);
        }
    }

}
