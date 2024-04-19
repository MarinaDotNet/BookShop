using Asp.Versioning;
using BookShop.API.Controllers.Services;
using BookShop.API.Models;
using BookShop.API.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;

namespace BookShop.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("account/")]
    public class OrderController(ILogger<OrderController> logger, UserManager<ApiUser> userManager, StockDBServices stockServices, AuthenticationApiDbContext dbContext) : ControllerBase
    {
        private readonly ILogger<OrderController> _logger = logger;
        private readonly UserManager<ApiUser> _userManager = userManager;
        private readonly StockDBServices _stockServices = stockServices;
        private readonly AuthenticationApiDbContext _dbContext = dbContext;

        #region of Help Methods
        private void LogingError(Exception error) => _logger.LogError(message: error.Message, args: error.StackTrace);
        private void LogingInformation(string message) => _logger.LogInformation(message: message);
        private void LogingWarning(string message) => _logger.LogWarning(message: message);
        private ActionResult Warning(string message, int statusCode)
        {
            LogingWarning(message);
            return statusCode == (int)HttpStatusCode.Unauthorized ?
                Unauthorized(message) :
                statusCode == (int)HttpStatusCode.NotFound ? 
                NotFound(message) :
                statusCode == (int)HttpStatusCode.BadRequest ?
                BadRequest(message):
                Problem(message);
        }
        private ActionResult Successfull(string message)
        {
            LogingInformation(message);
            return Ok(message);
        }
        private ActionResult Error(Exception ex)
        {
            LogingError(ex);
            return Problem(ex.Message.ToString());
        }
        private string GetCurrentUserName()
        {
            try
            {
                HttpContextAccessor accessor = new();
                var name = accessor.HttpContext!.User.Identity!.Name!;
                return name;
            }
            catch (Exception ex)
            {
                LogingError(ex);
                return string.Empty;
            }
        }
        #endregion

        //Creates new order, if there is not unsubmitted orders, for the current authorized user,
        //else returns Problem() object result with information message that contains
        //id for unsubmitted order.
        //User, should add new products to last not submitted order and submit it,
        //before to start another new order.
        [HttpPost, Route("/order")]
        public async Task<ActionResult> PostOrder([FromForm]List<string> productsIds)
        {
            try
            {
                //checks if user is signed in to create order
                var user = await _userManager.FindByNameAsync(GetCurrentUserName());
                if (user is not null)
                {                    
                    var listOfOrders = await _dbContext.Orders.Where(_ => _.UserId.Equals(user!.Id)).ToListAsync();
                    //checks if user has an uncompleted orders before to create new order
                    if(listOfOrders.Any(_ => !_.SubmittedOrder))
                    {
                        return Warning("Please finish previous order with id: " +
                            listOfOrders.Find(_ => !_.SubmittedOrder)!.OrderId, 0);
                    }

                    Order orderToPost = new()
                    {
                        OrderId = Guid.NewGuid().ToString(),
                        User = user,
                        UserId = user.Id,
                        TotalPrice = 0,
                        OrderDateTime = DateTime.Now,
                        SubmittedOrder = false
                    };
                    //checks if provided products ids exists and if they are available in stock
                    //adds the ids of products to the order
                    foreach (string id in productsIds)
                    {
                        Product product = await _stockServices.GetBookByIdAsync(id);

                        if (product is not null && product.IsAvailable)
                        {
                            orderToPost.ProductsId!.Add(product.Id!);
                            orderToPost.TotalPrice += product.Price;
                        }
                        else
                        {
                            string message = "The product with ID: " + id;
                            message = product is null ? 
                                ", was not found in stock, please check if ID is correct." :
                                ", currently is unavailble.";
                            return Warning(message + " Unable to process your order.", (int)HttpStatusCode.NotFound);
                        }
                    }

                    _dbContext.Orders.Add(orderToPost);
                    var result = await _dbContext.SaveChangesAsync();

                    string info = "OrderID: " + orderToPost.OrderId + ".For UserID: " + user.Id + "at DateTime: " + orderToPost.OrderDateTime;
                    if (result == 0)
                    {
                        LogingWarning("Unable to process request. Order was not saved " + info);
                        return BadRequest("Not able to process your request. Order was not saved.");
                    }
                    else return Successfull("Created successfully, " + info);
                }
                return Warning("User was not found in system, please ensure that you signed in", (int)HttpStatusCode.BadRequest);
            }
            catch(Exception ex)
            { 
                return Error(ex); 
            }
        }

        //Adds more products to existing order for the current authorized user
        [HttpPut, Route("/order/products/add")]
        public async Task<ActionResult> PutOrderAddProducts([FromForm][Required]List<string> productsIds, [Required]string orderId)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(GetCurrentUserName());
                string message = "";
                if (user is not null)
                {
                    Order ? order = await _dbContext.Orders.FirstOrDefaultAsync(order => order.UserId.Equals(user.Id) && order.OrderId.Equals(orderId));

                    //Checks if order with requested id exists and is not submitted yet
                    if(order is not null && !order.SubmittedOrder)
                    {
                        foreach (string id in productsIds)
                        {
                            Product product = await _stockServices.GetBookByIdAsync(id);

                            if (product is not null && product.IsAvailable)
                            {
                                order.ProductsId!.Add(product.Id!);
                                order.TotalPrice += product.Price;
                            }
                            else
                            {
                                message = "The product with ID: " + id;
                                message += product is null ?
                                    ", was not found in stock, please check if ID is correct." :
                                    ", currently is unavailble.";
                                return Warning(message + " Unable to process your request.", (int)HttpStatusCode.NotFound);
                            }
                        }
                        order.OrderDateTime = DateTime.Now;
                        _dbContext.Orders.Update(order);
                        var result = await _dbContext.SaveChangesAsync();

                       string info = "OrderID: " + order.OrderId + ".For UserID: " + user.Id + "at DateTime: " + order.OrderDateTime;
                           
                        if (result == 0)
                        {
                            return Warning("Unable to process request. Products was not added " + 
                                info, (int)HttpStatusCode.BadRequest);
                        }
                        else return Successfull("Products added successfully, " + info);
                    }
                    else
                    {
                        message = "Sorry, the requested order, with id: " + orderId;
                        message += order is not null ?
                         ", already submitted. Please start new order." :
                         ", was not found. Please start new order.";
                        return Warning(message + "Request declined at: " + DateTime.Now, (int)HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    return Warning("User was not found in system, please ensure that you signed in", (int)HttpStatusCode.BadRequest);
                }
            }
            catch(Exception ex)
            {
                return Error(ex);
            }
        }
    }
}
