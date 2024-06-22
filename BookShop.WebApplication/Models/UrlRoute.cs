
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

        public Uri CountBySearchTearm(string term, int page, int quantityPerPage, bool isAscending)
        {
            string toSearch = term.Replace(" ", "%20");
            string link = "https://localhost:7257/books/filter/count/condition?condition=" + toSearch;

            link += "&RequestedPage=";
            link += page == 0 ? "1" : page;

            link += "&QuantityPerPage=";
            link += quantityPerPage == 0 ? "6" : quantityPerPage;

            link += "&InAscendingOrder=" + isAscending + "&OrderBy=Price";

            return new Uri(link);
        }
        public Uri GetBySearchTearm (string term, int page, int quantityPerPage, bool isAscending)
        {
            string toSearch = term.Replace(" ", "%20");
            string link = "https://localhost:7257/books/filter/condition/contains?condition=" + toSearch;
            
            link += "&RequestedPage=";
            link += page == 0 ? "1" : page;

            link += "&QuantityPerPage=";
            link += quantityPerPage == 0 ? "6" : quantityPerPage;

            link += "&InAscendingOrder=" + isAscending + "&OrderBy=Price";

            return new Uri(link);
        }

        public Uri GetByGenre(string genre, int page, int quantityPerPage, bool isAscending)
        {
            string link = "https://localhost:7257/books/genre?genre=" + genre.Replace(" ", "%20");

            link += "&RequestedPage=";
            link += page == 0 ? "1" : page;

            link += "&QuantityPerPage=";
            link += quantityPerPage == 0 ?
                6 : quantityPerPage;

            link += "&InAscendingOrder=" + isAscending + "&OrderBy=Price";

            return new Uri(link);
        }
        public Uri CountByGenre(string genre)
        {
            string link = "https://localhost:7257/books/count/ingenre?genre=" + genre.Replace(" ", "%20");
            return new Uri(link);
        }
        public Uri GetByFilter(bool isAscending, int page, int quantityPerPage)
        {
            string link = "https://localhost:7257/books/page?RequestedPage=";
            link += page == 0 ? "1" : page;
            link += "&QuantityPerPage=";
            link += quantityPerPage == 0 ? "6" : quantityPerPage;
            link += "&InAscendingOrder=" + isAscending + "&OrderBy=Price";

            return new Uri(link);
        }
    }

    /**
    * Url's requests to the API OrderController
    * */
    public class UrlOrderRoute

    {
        public Uri GetAllOrders { get; } = new Uri("https://localhost:7257/order/all");

        //Url to the current unsubmitted yet order for signed in user
        public Uri GetCurrentOrder { get; } = new Uri("https://localhost:7257/order/current");
        //Url to check if currently signed in user has or not unsubmitted orders
        public Uri IsAnyUnsubmitted { get; } = new Uri("https://localhost:7257/order/is-any");
        
        public Uri PostNewOrder { get; set; } = new Uri("https://localhost:7257/order");

        public Uri DeleteProductsFromOrder(List<string>productsIds, string orderId)
        {
            string link = "https://localhost:7257/order/products/delete?";
            if(productsIds.Count > 0)
            {
                foreach(string productId in productsIds)
                {
                    link += "productIds=" + productId + "&";
                }
            }
            link += "orderId=" + orderId;
            return new Uri(link); 
        }

        /**
         * Generates and returns Uri for adding products into Order object where Order.OrderId == orderId
         * 
         * @List<string> productsIds - products ids to add into Order object
         * @string orderId - id of Order object
         * @returns Uri
         */
        public Uri AddProductsToOrder(List<string>productsIds, string orderId)
        {
            string link = "https://localhost:7257/order/products/add?";
            if(productsIds.Count > 0)
            {
                productsIds.ForEach(id => link += "productsIds=" + id + "&");
            }
            link += "orderId" + orderId;
            return new Uri(link);
        }
    }
}
