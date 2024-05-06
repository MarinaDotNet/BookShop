
using Microsoft.Identity.Client;

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

        //returns product by requested id, if id is valid and product  is available in stock
        //has access for not signed in users
        //must add valid id into query
        public Uri GetProductById { get; } = new Uri("https://localhost:7257/book/id");

        //return quantity of all products in database with default filters
        public Uri GetQuantityAll { get; } = new Uri("https://localhost:7257/books/count/all");

        //returns list of products
        public Uri GetAllProductsStandard(int page, int quantity, bool isAscending)
        {
            return new Uri("https://localhost:7257/books/page?RequestedPage=" + page + "&QuantityPerPage=" + quantity + "&InAscendingOrder=" + isAscending + "&OrderBy=Price");
            
        }

        //List of all genres
        public Uri GetListGenres { get; } = new Uri("https://localhost:7257/books/genres");
    }

    /**
    * Url's requests to the API OrderController
    * */
    public class UrlOrderRoute

    {

    }
}
