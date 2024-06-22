using Azure;
using BookShop.WebApplication.Areas.Identity.Pages.Account;
using BookShop.WebApplication.Models;
using BookShop.WebApplication.Models.ViewsModels;
using BookShop.WebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Versioning;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookShop.WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IMemoryCache _tokenCache;
        private IMemoryCache _orderCache;
        private readonly HttpClient _httpClient = new();
        private readonly IConfiguration _configuration;
        private ViewModel _viewModel = new();
        public HomeController(ILogger<HomeController> logger, 
            IMemoryCache tokenCache,
            IConfiguration configuration,
            IMemoryCache orderCache)
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

            _orderCache = orderCache;
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
                    throw new Exception();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    ProductViewModel productViewModel = new()
                    {
                        Product = _httpClient.GetFromJsonAsync<Product>(UriForQueryProductById(id)).Result!
                    };

                    return productViewModel.Product is null ? 
                        throw new Exception() : 
                        View(productViewModel);
                }
                else throw new Exception();
            }
            catch(Exception error)
            {
                Console.WriteLine(error.Message);
                return Error();
            }
        }

        [Authorize]
        public IActionResult AddToOrder(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id))
                {
                    throw new Exception();
                }

                Order orderCashe = (Order)_orderCache.Get("orderLast")!;

                if(orderCashe is null)
                {
                    orderCashe = GetLastOrder()!;
                    ManageOrderCashe(orderCashe);
                }

                var result = _httpClient.PutAsJsonAsync<Order>(new UrlOrderRoute().AddProductsToOrder([id], orderCashe.OrderId!), orderCashe).Result;

                return result.StatusCode == HttpStatusCode.OK ? 
                    RedirectToAction("Order") : 
                    throw new Exception();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);  
                return Error();
            }
        }

        [Authorize]
        public IActionResult Order()
        {
            try
            {
                //getting data about current unsubmitted order
                OrderViewModel current = new()
                {
                    Order = GetLastOrder()!
                };

                //calls method to check and fix if any order data was changed since it last viewed by user
                OrderViewModel order = CheckOrderData(current);

                //ensure that there nor dublicates in list of OrderViewModel.Order.Products
                if(order.Order.Products!.Count() > 1)
                {
                    order.Order.Products = ManageDublicateProducts(order.Order.Products!.ToList());
                }

                _viewModel.OrderViewModel = order;

                //updating the cache if in needs
                ManageOrderCashe(order.Order);

                return View(_viewModel);
            }
            catch (Exception error) 
            {
                Console.WriteLine(error.Message);
                return Error();
            }
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

        [Authorize]
        public IActionResult DeleteFromOrder(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception();
                }

                Order orderCashe = (Order)_orderCache.Get("orderLast")!;

                if (orderCashe is null)
                {
                    orderCashe = GetLastOrder()!;
                    ManageOrderCashe(orderCashe);
                }

                var result = _httpClient.PutAsJsonAsync<Order>(new UrlOrderRoute().DeleteProductsFromOrder([id], orderCashe.OrderId!), orderCashe).Result;

                return result.StatusCode == HttpStatusCode.OK ?
                    RedirectToAction("Order") :
                    throw new Exception();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        private Uri UriForQueryProductById(string id)
        {
            var queryData = new Dictionary<string, string> { ["id"] = id };

            return new(QueryHelpers.AddQueryString(new UrlStockRoute().GetProductById.ToString(), queryData!));
        }

        private Order RemoveProductsFromOrder(string orderId, List<string> productsId)
        {
            try
            {
                if(string.IsNullOrEmpty(orderId) || productsId.Count > 1)
                {
                    throw new Exception("Order was not found or problem with product/products with requested Ids");
                }

                var result = _httpClient.PutAsJsonAsync<string>(new UrlOrderRoute().DeleteProductsFromOrder(productsId, orderId), "").Result;
                
                var updated = _httpClient.GetFromJsonAsync<Order>(new UrlOrderRoute().GetCurrentOrder).Result;
                return updated is not null ? updated : new Order();
                
            }
            catch(Exception error)
            {
                Console.WriteLine(error.Message);
                Order newOrder = new()
                {
                    Notes = "Impossible to load requested order. Please contact to the Support."
                };
                return newOrder;
            }
        }
        //Checks and Fixes if in order any currently unavalable products
        private OrderViewModel CheckOrderData(OrderViewModel order)
        {
            try
            {
                if (order is null)
                {
                    throw new Exception();
                }
                if (order.Order.ProductsId is not null)
                {
                    //list to hold products from order that is still available
                    List<Product> productsAdd = [];
                    //list to hold products from order that is currently not available
                    List<string> toRemove = [];
                    foreach (string id in order.Order.ProductsId)
                    {
                        var res = _httpClient.GetAsync(UriForQueryProductById(id)).Result.StatusCode;
                        if (res.Equals(HttpStatusCode.OK))
                        {
                            productsAdd.Add(_httpClient.GetFromJsonAsync<Product>(UriForQueryProductById(id)).Result!);
                        }
                        else
                        {
                            toRemove.Add(id);
                        }
                    }
                    if (toRemove.Count > 0)
                    {
                        order.Order = RemoveProductsFromOrder(order.Order.OrderId!, toRemove);

                        string note = "Some products from your order currently not available.\t" +
                                "Some products was removed from your order. \t" +
                                "Products with id's: \t";

                        foreach(string id in toRemove)
                        {
                            note += id + "\t";
                        }
                        order.Order.Notes = note;
                    }
                    if (productsAdd is not null)
                    {
                        order.Order.Products = productsAdd;
                    }
                }
                return order;
            }
            catch(Exception error)
            {
                Console.WriteLine(error.ToString());
                OrderViewModel newOrder = new();
                newOrder.Order.Notes = "Impossible to load requested order. Please contact to the Support.";
                return newOrder;
            }
        }

        /**
         * returns existing unsubmitted Order or creates new Order if there is none and returns it
         * 
         * @return Order order or
         * @return null;
         * @throw ArgumentNullException() 
         */
        private Order? GetLastOrder()
        {
            try
            {
                Order? order = new();

                if (_httpClient.GetFromJsonAsync<bool>(new UrlOrderRoute().IsAnyUnsubmitted).Result!)
                {
                   order = _httpClient.GetFromJsonAsync<Order>(new UrlOrderRoute().GetCurrentOrder).Result!;
                }
                else
                {
                    var newData = _httpClient.PostAsJsonAsync<string>(new UrlOrderRoute().PostNewOrder, "").Result!;
                    if (newData.StatusCode == HttpStatusCode.OK)
                    {
                        order = _httpClient.GetFromJsonAsync<Order>(new UrlOrderRoute().GetCurrentOrder).Result!;
                    }
                    else throw new Exception(newData.ReasonPhrase);
                }
                return string.IsNullOrEmpty(order.OrderId) ? throw new ArgumentNullException() : order;
            }
            catch(Exception error)
            {
                Console.WriteLine(error.ToString());
                return null;
            }
        }

        /**
         * Updates value for key "orderLast" in _orderCashe
         * 
         * "orderLast" key holds the value of current unsubmitted yet Order
         * 
         */
        private void ManageOrderCashe(Order order)
        {
            try
            {
                Order orderLast = (Order)_orderCache.Get("orderLast")!;
                if (orderLast is null || 
                    string.IsNullOrEmpty(orderLast.OrderId) || 
                    !orderLast.OrderId!.Equals(_viewModel.OrderViewModel.Order.OrderId))
                {
                    orderLast = _orderCache.Set("orderLast", _viewModel.OrderViewModel.Order);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        /**
         * Ensure that there nor dublicates in list of Product
         * For each dublicate the parameter Product.QuantityInOrder should be incremented
         * 
         * @List<Product> entryData
         * @returns List<Product> returnData
         * @throws Exception()
         */
        private List<Product> ManageDublicateProducts(List<Product> entryData)
        {
            try
            {
                List<Product> returnData = [];
                if(entryData.Count > 0)
                {
                    foreach (Product product in entryData)
                    {
                        if (returnData.Where(_ => _.Id!.Equals(product.Id)).Count() == 1)
                        {
                            int index = returnData.IndexOf(returnData.Where(_ => _.Id!.Equals(product.Id)).FirstOrDefault()!);
                            returnData.ElementAt(index).QuantityInOrder += 1;
                        }
                        else
                        {
                            returnData.Add(product);
                        }
                    }
                    return returnData.Count > 0 ? returnData : throw new Exception();
                }
                throw new Exception();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return [];
            }
        }
        #endregion
    }
}
