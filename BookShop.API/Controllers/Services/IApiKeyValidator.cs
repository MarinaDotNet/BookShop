/*
 * Interface to validate Api Key
 */
namespace BookShop.API.Controllers.Services
{
    public interface IApiKeyValidator
    {
        bool isValid(string apiKey);
    }
}
