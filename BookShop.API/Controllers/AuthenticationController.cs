using Asp.Versioning;
using BookShop.API.Controllers.Services;
using BookShop.API.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
                        LogingWarning((@"Unauthorized access rejected for: {@user}, at {@DateTime}", user, DateTime.Now).ToString());
                        return Unauthorized("Access denied for user: '" + user.UserName + "', current user does not have authority");
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

                LogingInformation((@"Unauthorized access rejected for: {@user}, at {@DateTime}", user, DateTime.Now).ToString());
                return Unauthorized(user is null ? "Not registrated user:'" + model.UserLogin + "'." : "Entered incorrect password");
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
                    string message = emailExists is not null && userNameExists is not null ?
                        "User with this email and login already registrated" :
                        emailExists is not null ?
                        "User with this email already registrated" :
                        "User with this login already registrated";
                    LogingWarning(message);
                    return Problem(message);
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
                    string message = (@"Unable registrate user with entered details: {@user} at {@DateTime}. Please check entered details.", user, DateTime.Now).ToString();
                    LogingWarning(message);
                    return Problem(message);
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

                LogingInformation((@"Registrated Successfully: {@apiUser}, with roles: {@roles}, at {@DateTime}", user, roles, DateTime.Now).ToString());

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
    }

}
