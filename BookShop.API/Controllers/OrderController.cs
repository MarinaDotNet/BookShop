using Asp.Versioning;
using BookShop.API.Controllers.Services;
using BookShop.API.Models;
using BookShop.API.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Bson;
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

        private string MessageUnavailableProducts(List<string>productsIdsNotFound, bool orderIsSubmitted)
        {
            string message = "";
            if(productsIdsNotFound.Any())
            {
                message += "Some product from your order, currently unavailable in stock.";
                message += orderIsSubmitted ?
                    "There no detailed data can be displayed for products with IDs: " :
                    "Total Order price recounted and products was removed from your Order." +
                    "Removed Products IDs: ";
                int index = productsIdsNotFound.Count;
                foreach (string id in productsIdsNotFound)
                {
                    --index;
                    message += index != 0 ? id + ", " : id + ".";
                }
            }
            return message;   
        }
        #endregion

        //Creates new order, if there is not unsubmitted orders, for the current authorized user,
        //else returns Problem() object result with information message that contains
        //id for unsubmitted order.
        //User, should add new products to last not submitted order and submit it,
        //before to start another new order.
        [HttpPost, Route("/order")]
        public async Task<ActionResult<OrderDisplayModel>> PostOrder([FromForm]List<string> productsIds)
        {
            try
            {
                string message = "";
                //checks if user is signed in to create order
                var user = await _userManager.FindByNameAsync(GetCurrentUserName());
                if (user is not null)
                {
                    var listOfOrders = await _dbContext.Orders.Where(_ => _.UserId.Equals(user!.Id)).ToListAsync();
                    //checks if user has an uncompleted orders before to create new order
                    if (listOfOrders.Any(_ => !_.SubmittedOrder))
                    {
                        Order ? order = listOfOrders.FirstOrDefault(_ => !_.SubmittedOrder);
                        message = "Please submit order above before to create new order";
                        OrderDisplayModel model = new(order!, message);
                        return Warning(model.ToJson(), 0);
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
                            message += "The product with ID: " + id;
                            message += product is null ?
                                ", was not found in stock, please check if ID is correct." :
                                ", currently is unavailble.";
                            return Warning(message + " Unable to process your order.", (int)HttpStatusCode.NotFound);
                        }
                    }

                    _dbContext.Orders.Add(orderToPost);
                    var result = await _dbContext.SaveChangesAsync();

                    if (result == 0)
                    {
                        LogingWarning("Unable to process request. Order was not saved.");
                        return BadRequest("Not able to process your request. Order was not saved.");
                    }
                    else
                    {
                        message = "Order created successfully";
                        OrderDisplayModel model = new(orderToPost, message);
                        return Successfull(model.ToJson());
                    }
                        
                }
                return Warning("User was not found in system, please ensure that you signed in", (int)HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        //Adds more products to existing order for the current authorized user
        [HttpPut, Route("/order/products/add")]
        public async Task<ActionResult<OrderDisplayModel>> PutOrderAddProducts([FromForm][Required]List<string> productsIds, [Required]string orderId)
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
                        //Checks if previously added products in order are still available
                        List<string> productsNotAvailable = new();
                        foreach(string id in order.ProductsId!)
                        {
                            Product product = await _stockServices.GetBookByIdAsync(id);
                            if(product is null || !product.IsAvailable)
                            {
                                productsNotAvailable.Add(id);
                            }
                        }
                        if(productsNotAvailable.Any())
                        {
                            message += MessageUnavailableProducts(productsNotAvailable, order.SubmittedOrder);

                            order.ProductsId = 
                                [.. order.ProductsId.Where(id => !productsNotAvailable.Contains(id))];
                        }
                        //Checks if currently requested products are in stock
                        foreach (string id in productsIds)
                        {
                            Product product = await _stockServices.GetBookByIdAsync(id);

                            if (product is not null && product.IsAvailable)
                            {
                                order.ProductsId!.Add(product.Id!);
                            }
                            else
                            {
                                message += "The product with ID: " + id + ", was not added to the order, because product ";
                                message += product is null ?
                                    " was not found in stock, please check if ID is correct." :
                                    " currently is unavailble.";
                            }
                        }
                        //recounts/resets ordr total price
                        decimal price = 0;
                        foreach (string id in order.ProductsId)
                        {
                            Product product = await _stockServices.GetBookByIdAsync(id);
                            price += product.Price;
                        }
                        order.TotalPrice = price;
                        order.OrderDateTime = DateTime.Now;
                        _dbContext.Orders.Update(order);
                        var result = await _dbContext.SaveChangesAsync();
 
                        if (result == 0)
                        {
                            return Warning("Unable to process request. Products was not added", (int)HttpStatusCode.BadRequest);
                        }
                        else
                        {
                            message += " Order updated successfully";
                            OrderDisplayModel model = new(order, message);
                            return Successfull(model.ToJson()); 
                        }
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

        //Deletes products from existing order for the current authorized user
        [HttpPut, Route("/order/products/delete")]
        public async Task<ActionResult> PutOrderDeleteProducts([FromForm][Required]List<string> productIds, [Required]string orderId)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(GetCurrentUserName());
                string message = "";
                if (user is not null)
                {
                    Order? order = await _dbContext.Orders.FirstOrDefaultAsync(order => order.UserId.Equals(user.Id) && order.OrderId.Equals(orderId));

                    //Checks if order with requested id exists and is not submitted yet
                    if (order is not null && !order.SubmittedOrder)
                    {
                        foreach(string id in productIds)
                        {
                            if(order.ProductsId!.Contains(id))
                            {
                                decimal price = (await _stockServices.GetBookByIdAsync(id)).Price;
                                order.ProductsId.Remove(id);
                                order.TotalPrice -= price;
                            }
                        }
                        order.OrderDateTime = DateTime.Now;
                        _dbContext.Orders.Update(order);
                        var result = await _dbContext.SaveChangesAsync();

                        string info = "OrderID: " + order.OrderId + ".For UserID: " + user.Id + "at DateTime: " + order.OrderDateTime;

                        if (result == 0)
                        {
                            return Warning("Unable to process request. Products was not removed " +
                                info, (int)HttpStatusCode.BadRequest);
                        }
                        else return Successfull("Products removed successfully, " + info);
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
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        //Submitts orders
        //Checks if there all products still available in stock before submmiting.
        //If some products is not available, then deletes it from order, recounting total price and
        //informing the user about this and that the order needs to be double-checked and resubmit it
        [HttpPut, Route("/order/submit")]
        public async Task<ActionResult> PutOrderAsSubmitted([FromForm][Required]string orderId)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(GetCurrentUserName());
                string message = "";
                if (user is not null)
                {
                    Order? order = await _dbContext.Orders.FirstOrDefaultAsync(order => order.UserId.Equals(user.Id) && order.OrderId.Equals(orderId));

                    //Checks if order with requested id exists and is not submitted yet
                    if (order is not null && !order.SubmittedOrder)
                    {
                        //Checks if all products in order exists and available
                        if(order.ProductsId!.Any())
                        {
                            string idsDeleted = "";
                            List<string> productsIds = order.ProductsId!.ToList();
                            List<string> productsIdNotAvailable = new();
                            foreach (string productId in productsIds)
                            {
                                Product product = await _stockServices.GetBookByIdAsync(productId);
                                if(product is null || !product.IsAvailable)
                                {
                                    idsDeleted += productId + ", ";
                                    productsIdNotAvailable.Add(productId);
                                    order.ProductsId!.Remove(productId);
                                }
                            }
                            //Recounts total price for order if there was removed any not in stock products
                            if(productsIdNotAvailable.Any())
                            {
                                decimal totalPrice = 0;
                                foreach (string producId in productsIds)
                                {
                                    Product product = await _stockServices.GetBookByIdAsync(producId);
                                    totalPrice = product is not null && product.IsAvailable ?
                                        totalPrice + product.Price : totalPrice + 0;
                                }
                                order.TotalPrice = totalPrice;
                            }
                            //Resetting order 
                            order.SubmittedOrder = !productsIdNotAvailable.Any();
                            order.OrderDateTime = DateTime.Now;
                            _dbContext.Orders.Update(order);

                            var result = await _dbContext.SaveChangesAsync();

                            //Checks the result 
                            if(result == 0)
                            {
                                LogingWarning("Unable to process request. Order was not saved, OrderID" + order.OrderId);
                                return BadRequest("Not able to process your request. Order was not saved.");
                            }
                            else
                            {
                                //if there was deleted any products from Order, needs user to recheck what is
                                //in it before submitting
                                if (!order.SubmittedOrder)
                                {
                                    message = "The product/products with id: " + idsDeleted + ", was not found or        currently unavailable. Those products was removed from your order. Order was not subbmitted. Please recheck the order and resubmit it again.";
                                    return Warning(message, (int)HttpStatusCode.NotFound);
                                }
                                else
                                {
                                    return Successfull("Order with id: " + order.OrderId + ", submitted successfully, at: " + order.OrderDateTime);
                                }
                            }
                        }
                        else
                        {
                            return Warning("Can not submit empty Order, Order should have at list one product. Request declined at: " + DateTime.Now, (int)HttpStatusCode.BadRequest);
                        }
                    }
                    else
                    {
                        message = "Sorry, the requested order, with id: " + orderId;
                        message += order is not null ?
                         ", already submitted." :
                         ", was not found.";
                        return Warning(message + "Request declined at: " + DateTime.Now, (int)HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    return Warning("User was not found in system, please ensure that you signed in", (int)HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        [HttpGet, Route("/order/details")]
        public async Task<ActionResult<OrderDisplayModel>> GetOrder([Required]string orderId)
        {
            try
            {
                string info = "";
                var user = await _userManager.FindByNameAsync(GetCurrentUserName());
                if(user is not null)
                {
                    Order? order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId.Equals(orderId));
                    if (order is not null)
                    {
                        List<Product> productsAvailable = new();
                        List<string> productNotAvailable = new();
                        foreach(string productId in order.ProductsId!)
                        {
                            Product product = await _stockServices.GetBookByIdAsync(productId);
                            if(product is not null && product.IsAvailable)
                            {
                                productsAvailable.Add(product);
                            }
                            else
                            {
                                productNotAvailable.Add(productId);
                            }
                        }
                        //if in current order.ProductsId any unavailable products and order not submitted yet,
                        //recounts total price and updates order.ProductsId
                        if (productNotAvailable.Any() && !order.SubmittedOrder)
                        {
                            decimal priceUpdate = 0;
                            order.ProductsId.Clear();
                            foreach (Product product in productsAvailable)
                            {
                                order.ProductsId.Add(product.Id!);
                                priceUpdate += product.Price;
                            }

                            order.TotalPrice = priceUpdate;
                            _dbContext.Orders.Update(order);
                            var result = await _dbContext.SaveChangesAsync();

                            if (result == 0)
                            {
                               string message = "Some inner error occured. Unable to process your request. Please tyr latter or contact to supporting team.";
                                return Warning(message, 0);
                            }
                            info = MessageUnavailableProducts(productNotAvailable, order.SubmittedOrder);
                        } 

                        OrderDisplayModel display = new(order, info);
                        return Successfull(display.ToJson());
                    }
                    else
                    {
                        return Warning("Order with Id: " + orderId + ", was not found. UserID: " + user.Id + ", access declined at: " + DateTime.Now, (int)HttpStatusCode.NotFound);
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
