using Asp.Versioning;
using BookShop.API.Controllers.Services;
using BookShop.API.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace BookShop.API.Controllers
{
    /*
     * Version for Admin to retrive all data and manipulates with data
     * Supports all methods: HttpGet, HttpPost, HttpPut and HttpDelete
     */
    [ApiController]
    [ApiVersion("1")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class StockV1Controller(StockDBServices services, ILogger<StockV1Controller> logger) : ControllerBase
    {
        private readonly StockDBServices _services = services;
        private readonly ILogger<StockV1Controller> _logger = logger;

        #region of HttpGet Methods
        #region of simple HttpGet Methods
        /*!ATTENTION! may be overflow error, or slowdown preformance.Depends on current size of database
         * returns all data from database collection
        */
        [HttpGet, Route("books/all")]
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            try
            {
                var products = await _services.GetAllBooksAsync();

                return products is null || products.Count == 0 ?
                    NotFound() : Ok(products);
            }
            catch (Exception ex)
            {
                LoggError(ex.Message.ToString(), ex.StackTrace!);
                return Problem(ex.Message);
            }
        }

        //returns list of requested quantity of products for requested page
        //list sorted in requested order, by requested parameter(by Title or by Author or by Price)
        //if not specified sorting order && parameter for order, then its uses default parameters:
        //descending order by Price
        [HttpGet, Route("books/page")]
        public async Task<ActionResult<List<Product>>> GetPerPageProducts([FromQuery] PageModel model)
        {
            try
            {
                Query query = new(model.RequestedPage, model.QuantityPerPage, (int)GetQuantityAllProducts().Result);
                var products = (await _services.GetBooksInOrder(model.InAscendingOrder, model.OrderBy)).Skip(query.QuantityToSkip).Take(query.RequestedQuantity);

                return products is null || !products.Any() ?
                    NotFound("There no products found under entered requirements") : Ok(products);
            }
            catch (Exception ex)
            {
                LoggError(ex.Message.ToString(), ex.StackTrace!);
                return Problem(ex.Message);
            }
        }

        #endregion
        #endregion
        #region of HttpMethods for manipulations with Collection
        [HttpPost, Route("book/add")]
        public async Task<ActionResult> PostProduct([FromQuery]Product product)
        {
            try
            {
                if(product.Genres is null || product.Genres.Length == 0)
                {
                    product.Genres = ["unspecified"];
                }
                Product book = new()
                {
                    Title = product.Title,
                    Author = product.Author,
                    Annotation = product.Annotation,
                    IsAvailable = product.IsAvailable,
                    Language = product.Language,
                    Link = !string.IsNullOrEmpty(product.Link.ToString()) && Uri.IsWellFormedUriString(product.Link.ToString(), UriKind.Absolute) ?
                    product.Link : new Uri("about:blank"),
                    Genres = [.. product.Genres],
                    Price = product.Price > 0 ? product.Price : 0
                };
                await _services.AddNewAsync(book);

                LoggInfo(Ok().StatusCode, "Successfully added product, with id: '" + book.Id + "'. ");
                return Ok("Successfully added: " + book.ToJson());
            }
            catch (Exception ex)
            {
                LoggError(ex.Message.ToString(), ex.StackTrace!);
                return Problem(ex.Message);
            }
        }
        #endregion
        #region of Help Methods
        private void LoggError(string errorMessage, string errorStackTrace )
        {
            _logger.LogError(message: errorMessage, args: errorStackTrace);
        }
        private void LoggInfo(int statusCode, string message)
        {
            _logger.LogInformation(message: message + ", DateTime: {@DateTime}, StatusCode: {@statusCode}", DateTime.Now, statusCode);
        }

        #region of count methods
        //returns quantity of all products in database
        [HttpGet, Route("books/count/all")]
        public async Task<int> GetQuantityAllProducts()
        {
            try
            {
                int quantity = (await _services.GetAllBooksAsync()).Count;
                return quantity;
            }
            catch(Exception ex)
            {
                LoggError(ex.Message.ToString(), ex.StackTrace!);
                return 0;
            }
        }
        #endregion
        #endregion
    }

    /*
     * Version for Users to only retrive some data from database, where property 'available' == true
     * Supports only methods: HttpGet
     */
    [ApiController]
    [ApiVersion("2")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class StockV2Controller(StockDBServices services, ILogger<StockV2Controller> logger) : ControllerBase
    {
        private readonly StockDBServices _services = services;
        private readonly ILogger<StockV2Controller> _logger = logger;

        #region of HttpGet Methods
        #region of simple HttpGet Methods
        /*!ATTENTION! may be overflow error, or slowdown preformance.Depends on current size of database
         * returns all data from database collection
        */
        [HttpGet, Route("books/all")]
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            try
            {
                var products = (await _services.GetAllBooksAsync()).Where(_ => _.IsAvailable).ToList();

                if(products is null)
                {
                    LoggInfo(NotFound().StatusCode, "No data in database found with specified requerments");
                }
                return products is null || products.Count == 0 ?
                    NotFound() : Ok(products);
            }
            catch (Exception ex)
            {
                LoggError(ex.Message, ex.StackTrace!);
                return Problem(ex.Message);
            }
        }

        //returns list of requested quantity of products.isAvailable for requested page
        //list sorted in requested order, by requested parameter(by Title or by Author or by Price)
        //if not specified sorting order && parameter for order, then its uses default parameters:
        //descending order by Price
        [HttpGet, Route("books/page")]
        public async Task<ActionResult<List<Product>>> GetPerPageProducts([FromQuery] PageModel model)
        {
            try
            {
                Query query = new(model.RequestedPage, model.QuantityPerPage, (int)GetQuantityAllProducts().Result);
                var products = (await _services.GetBooksInOrder(model.InAscendingOrder, model.OrderBy))
                    .Where(_ => _.IsAvailable)
                    .Skip(query.QuantityToSkip).Take(query.RequestedQuantity);

                return products is null || !products.Any() ?
                    NotFound("There no products found under entered requirements") : Ok(products);
            }
            catch (Exception ex)
            {
                LoggError(ex.Message.ToString(), ex.StackTrace!);
                return Problem(ex.Message);
            }
        }

        #endregion
        #endregion

        #region of Help Methods
        private void LoggError(string errorMessage, string errorStackTrace)
        {
            _logger.LogError(message: errorMessage, args: errorStackTrace);
        }
        private void LoggInfo(int statusCode, string message)
        {
            _logger.LogInformation(message: message + ", DateTime: {@DateTime}, StatusCode: {@statusCode}", DateTime.Now, statusCode);
        }

        #region of count methods
        //returns quantity of all products.isAvailable in database
        [HttpGet, Route("books/count/all")]
        public async Task<int> GetQuantityAllProducts()
        {
            try
            {
                int quantity = (await _services.GetAllBooksAsync()).Where(_ => _.IsAvailable).ToList().Count;
                return quantity;
            }
            catch (Exception ex)
            {
                LoggError(ex.Message.ToString(), ex.StackTrace!);
                return 0;
            }
        }
        #endregion
        #endregion
    }
}
