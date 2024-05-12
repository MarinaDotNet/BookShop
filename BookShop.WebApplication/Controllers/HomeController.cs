using Azure;
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
using System.ComponentModel.DataAnnotations;
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
            return View(new ViewModel()
            {
                ErrorViewModel = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
            });
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
        public IActionResult Filtered(string genre, bool isAcsending, int page, int quantity)
        {
            try
            {
                PageViewModel pageModel = new()
                {
                    currentPage = page > 1 ? page : 1,
                    quantityPerPage = quantity > 6 ? quantity : 6
                };

                int totalProducts = _httpClient.GetFromJsonAsync<int>(
                    string.IsNullOrEmpty(genre) || genre.Equals("any") ?
                    new UrlStockRoute().GetQuantityAll : new UrlStockRoute().CountByGenre(genre)).Result!;
                if(totalProducts > 0)
                {
                    pageModel.SetTotalPages(totalProducts);
                }
                else
                {
                    return Error();
                }

                ProductViewModel productViewModel = new()
                {
                    Products = _httpClient.GetFromJsonAsync<IEnumerable<Product>>(
                        string.IsNullOrEmpty(genre) || genre.Equals("any") ?
                            new UrlStockRoute().GetByFilter(
                                isAcsending, pageModel.currentPage, pageModel.quantityPerPage) : 
                            new UrlStockRoute().GetByGenre(
                                genre, pageModel.currentPage, pageModel.quantityPerPage, isAcsending)).Result!
                };

                FilterModel filterModel = new()
                {
                    SearchTearm = string.Empty,
                    SelectedGenre = string.IsNullOrEmpty(genre) || genre.Equals("any") ?
                    string.Empty : genre,
                    IsAscendingOrder = isAcsending
                };

                ViewModel model = new()
                {
                    FilterViewModel = filterModel,
                    PageViewModel = pageModel,
                    ProductViewModel = productViewModel,
                    Genres = [..GetListGenres(new UrlStockRoute().GetListGenres)]
                };

                return model.ProductViewModel.Products is not null ?
                    View(model) : Error();
            }
            catch
            {
                return Error();
            }
        }


        [Authorize]
        public IActionResult Search(string search, int page, bool isAcsending, int quantity)
        {
            try
            {
                if(string.IsNullOrEmpty(search))
                {
                    return Error();
                }

                PageViewModel pageModel = new()
                {
                    currentPage = page > 1 ? page : 1,
                    quantityPerPage = quantity > 6 ? quantity : 6
                };

                int totalProducts = _httpClient.GetFromJsonAsync<int>(new UrlStockRoute().CountBySearchTearm(search, page, quantity, isAcsending)).Result!;
                if(totalProducts > 0)
                {
                    pageModel.SetTotalPages(totalProducts);
                }
                else
                {
                    return Error();
                }

                ProductViewModel productViewModel = new()
                {
                    Products = _httpClient.GetFromJsonAsync<IEnumerable<Product>>(new UrlStockRoute().GetBySearchTearm(search, pageModel.currentPage, pageModel.quantityPerPage, isAcsending)).Result!
                };

                FilterModel filter = new()
                {
                    SearchTearm = search,
                    IsAscendingOrder = isAcsending
                };

                ViewModel model = new()
                {
                    PageViewModel = pageModel,
                    ProductViewModel = productViewModel,
                    FilterViewModel = filter
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

        #region of Help Methods for Controller
        //Returns list of genres in Ascending order and all records converted to lowercase
        private List<string> GetListGenres(Uri url)
        {
            try
            {
                IEnumerable<string> data = _httpClient.GetFromJsonAsync<IEnumerable<string>>(url).Result!;
                
                List<string> genres = [];
                if(data.Any() && data is not null)
                {
                    data.Order().ToList().ForEach(_ => genres.Add(_.ToLowerInvariant()));
                }
                return genres.Count > 0 ? [.. genres.Order()] : genres;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, message: ex.Message, ex.StackTrace);
                return [];
            }
        }
        #endregion
    }
}
