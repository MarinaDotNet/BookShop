/**
 * Implements the IApiKeyValidator interface class IsValid(string)
 * and injecting the IConfiguration into the constructor
 **/
namespace BookShop.API.Controllers.Services
{
    public class ApiKeyValidator(IConfiguration configuration) : IApiKeyValidator
    {
        private readonly IConfiguration _configuration = configuration;

        public bool isValid(string key)
        {
            try
            {
                if(string.IsNullOrEmpty(key)) 
                { 
                    return false; 
                }
                string? keyConf = _configuration.GetValue<string>(ApiConstants.ApiKeyName);
                return !string.IsNullOrEmpty(keyConf) && key.Equals(keyConf);
            }
            catch
            {
                return false;
            }
        }
    }
}
