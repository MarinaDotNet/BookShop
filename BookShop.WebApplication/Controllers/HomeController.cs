using BookShop.WebApplication.Areas.Identity.Pages.Account;
using BookShop.WebApplication.Models;
using BookShop.WebApplication.Models.ViewsModels;
using BookShop.WebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BookShop.WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IMemoryCache _tokenCache;
        private readonly HttpClient _httpClient = new();
        private readonly IConfiguration _configuration;
        private ViewModel _viewModel = new();
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
                ProductViewModel products = new() 
                { 
                    Products = _httpClient.GetFromJsonAsync<IEnumerable<Product>>(new UrlStockRoute().GetFiveMostExpensiveProducts).Result!
                };

                _viewModel.ProductViewModel = products;

                return _viewModel.ProductViewModel.Products.Any() ?
                    View(_viewModel) :
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

                    Uri url = new(QueryHelpers.AddQueryString(new UrlStockRoute().GetProductById.ToString(), queryData!));

                    ProductViewModel productViewModel = new();
                    productViewModel.Product = _httpClient.GetFromJsonAsync<Product>(url).Result!;

                    return productViewModel.Product is null ? Error() : View(productViewModel);
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

        [Authorize]
        public IActionResult AddToOrder(string id)
        {
            return View();
        }

        [Authorize]
        public IActionResult Shop(int page)
        {
            try
            {
                PageViewModel pageModel = new();
                
                var quantity = _httpClient.GetFromJsonAsync<int>(new UrlStockRoute().GetQuantityAll).Result;
                if(quantity > 0)
                {
                    pageModel.SetTotalPages(quantity);
                }
                else
                {
                    return Error();
                }

                if(page > 1)
                {
                   pageModel.currentPage = page;
                }

                ProductViewModel productViewModel = new()
                {
                    Products = _httpClient.GetFromJsonAsync<IEnumerable<Product>>(new UrlStockRoute().GetAllProductsStandard(pageModel.currentPage, pageModel.quantityPerPage, true)).Result!
                };

                IEnumerable<string> genres = _httpClient.GetFromJsonAsync<IEnumerable<string>>(new UrlStockRoute().GetListGenres).Result!;

                ViewModel model = new()
                {
                    PageViewModel = pageModel,
                    ProductViewModel = productViewModel,
                    Genres = genres is not null ? genres.ToArray() : []
                };

                return model.ProductViewModel.Products.Any() ?
                    View(model) :
                    Error();
            }
            catch
            {
                return Error();
            }

        }

        [Authorize]
        public IActionResult Filtered(string genre, bool isAcsending)
        {
            return View();
        }


        [Authorize]
        public IActionResult Search(string search)
        {
            return View();
        }
    }
}
