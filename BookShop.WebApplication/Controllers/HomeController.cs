using BookShop.WebApplication.Models;
using BookShop.WebApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace BookShop.WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IMemoryCache _tokenCache;
        private readonly HttpClient _httpClient = new();
        private readonly IConfiguration _configuration;
        private ProductViewModel _productViewModel = new();
        public HomeController(ILogger<HomeController> logger, 
            IMemoryCache tokenCache,
            IConfiguration configuration)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _configuration = configuration;

            _httpClient.DefaultRequestHeaders.Add(ApplicationConstants.ApiKeyHeader,
                _configuration[ApplicationConstants.ApiKeyName]);

            if(_tokenCache.TryGetValue("token", out TokenGenerator? token))
            {
                _httpClient.DefaultRequestHeaders.Add(ApplicationConstants.ApiVersionHeader, ApplicationConstants.ApiVersionValue);
                _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token!.Value);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add(ApplicationConstants.ApiVersionHeader, "3");
            }
        }

        public IActionResult Index()
        {
            try
            {
                _productViewModel.Products = _httpClient.GetFromJsonAsync<IEnumerable<Product>>(new UrlStockRoute().GetFiveMostExpensiveProducts).Result!;

                return _productViewModel.Products.Any() ?
                    View(_productViewModel) :
                    Error();
            }
            catch
            {
                return Error();
            }

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Details(string id)
        {
            try
            {
                if (id is not null)
                {
                    var queryData = new Dictionary<string, string> { ["id"] = id };

                    Uri url = new (QueryHelpers.AddQueryString(new UrlStockRoute().GetProductById.ToString(), queryData!));

                    _productViewModel.Product = _httpClient.GetFromJsonAsync<Product>(url).Result!;

                    return _productViewModel.Product is null ? Error() : View(_productViewModel);
                }
                else
                {
                    return Error();
                }
            }
            catch
            {
                return Error();
            }
        }

        public IActionResult AddToOrder(string id)
        {
            return View();
        }

        public IActionResult More()
        {
            return View();
        }
    }
}
