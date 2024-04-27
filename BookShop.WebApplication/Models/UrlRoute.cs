/**
 * To store all url's for the requests to the API 
 **/
namespace BookShop.WebApplication.Models
{
    /**
    * Url's requests to the API StockController
    * */
    public class UrlStockRoute
    {
        //returns first 5 available  in stock products with highest price,
        //has access for not signed in users
        //for index page
        public Uri GetFiveMostExpensiveProducts { get; } = new Uri("https://localhost:7257/books/filter?OrderBy=Price");
    }

    /**
    * Url's requests to the API OrderController
    * */
    public class UrlOrderRoute

    {

    }
}
