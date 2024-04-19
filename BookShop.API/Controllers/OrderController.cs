using Asp.Versioning;
using BookShop.API.Controllers.Services;
using BookShop.API.Models;
using BookShop.API.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
                Problem(message);
        }
        #endregion

        //Creates new order, if there is not unsubmitted orders, for the current user,
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
                HttpContextAccessor accessor = new();
                var user = await _userManager.FindByNameAsync(accessor.HttpContext!.User.Identity!.Name!);
                if(user is not null)
                {                    
                    var listOfOrders = await _dbContext.Orders.Where(_ => _.UserId.Equals(user!.Id)).ToListAsync();
                    //checks if user has an uncompleted orders before to create new order
                    if(listOfOrders.Any(_ => !_.SubmittedOrder))
                    {
                        string message = "Please finish previous order with id: " +
                            listOfOrders.Find(_ => !_.SubmittedOrder).OrderId;
                        LogingWarning(message);
                        return Problem(message);
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
                            string message = product is null ? 
                                "The product with ID: " + id + ", was not found in stock, please check if ID is correct. Unable to process your request." :
                                "The product with ID: " + id + ", currently is unavailble. Unable to process your order.";
                            LogingWarning(message);
                            return NotFound(message);
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
                    else
                    {
                        LogingInformation("Order created successfully, " + info);
                        return Ok("Created successfully, " + info);
                    }
                }

                LogingWarning("User was not found in system, please ensure that you signed in");
                return BadRequest("User was not found in system, please ensure that you signed in");
            }
            catch(Exception ex)
            {
                LogingError(ex);
                return Problem(ex.Message.ToString());
            }
        }
    }
}
