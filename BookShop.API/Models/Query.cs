/*
 * Model for Query
 * To divide content(data from database) into pages
 */
namespace BookShop.API.Models
{
    public class Query
    {
        public const int MinPossibleQuantityAtPage = 5;
        public const int MaxPossibleQuantityAtPage = 30;
        public const int MinPossiblePage = 1;

        private readonly int requestedPage = MinPossiblePage;
        //total quantity of products in db
        private readonly int totalQuantity;
        //amount of all pages
        private readonly int totalPages;
        //requested quantity of products per page
        private readonly int requestedQuantityPerPage = MinPossibleQuantityAtPage;
        //posible maximum quantity of products per page, CAN'T BE OVER totalQuantity
        private readonly int possibleMaxQuantityPerPage = MaxPossibleQuantityAtPage;
        //quantity of products that needs to be skipped before those products that has to be displayed on the requested page
        private readonly int quantityToSkip = 0;
        public Query(int page, int quantity, int totalProductsQuantity)
        {
            totalQuantity = totalProductsQuantity;

            //finding out if requestedQuantityPerPage is adequate,
            //if it is negative or 0 value, then set to const MinPossibleQuantityAtPage
            //if it is over const MaxPossibleQuantityAtPage, then set value to const MaxPossibleQuantityAtPage
            requestedQuantityPerPage = quantity >= MinPossibleQuantityAtPage && quantity <= MaxPossibleQuantityAtPage ?
                quantity : 
                quantity > possibleMaxQuantityPerPage ? possibleMaxQuantityPerPage : MinPossibleQuantityAtPage;

            //if (totalQuantity <= 0) then totalPages = 0
            //else if (totalQuantity < requestedQuantityPerPage) then totalPages = 1
            //else if totalQuantity divided without remainder on requestedQuantityPerPage then
            //totalPages = totalQuantity / requestedQuantityPerPage
            //else totalPages = (totalQuantity / requestedQuantityPerPage) + 1
            totalPages = totalQuantity > 0 && totalQuantity > requestedQuantityPerPage ?
                ((totalQuantity % requestedQuantityPerPage) == 0 ? 
                totalQuantity / requestedQuantityPerPage : 
                totalQuantity / requestedQuantityPerPage + 1) :
                (totalQuantity <= 0 ? 0 : 1);

            //Here checks if totalQuantity of products not less of const MaxPossibleQuantityAtPage
            //conditionally possibleMaxQuantityPerPage equal to const MaxPossibleQuantityAtPage
            //if(totalQuantity < MaxPossibleQuantityAtPage) then possibleMaxQuantityPerPage = totalQuantity
            possibleMaxQuantityPerPage = totalQuantity > MaxPossibleQuantityAtPage ? 
                MaxPossibleQuantityAtPage : totalQuantity;

            //if requested page less then 1, then requestedPage shuld be first page
            //and if requested page over totalPages, then requestedPage shuld be last page
            requestedPage = page >= MinPossiblePage && page <= totalPages ?
                page :
                page > totalPages ? totalPages : MinPossiblePage;

            //sets the number of products that should be skipped before the products
            //that should be on the requested page
            quantityToSkip = requestedPage > MinPossiblePage ?
                requestedQuantityPerPage * (requestedPage - 1) : 0;

            RequestedPage = requestedPage;
            RequestedQuantity = requestedQuantityPerPage;
            TotalQuantity = totalQuantity;
            TotalPages = totalPages;
            PossibleMaxQuantity = possibleMaxQuantityPerPage;
            QuantityToSkip = quantityToSkip;
    }

        public int RequestedPage { get; private set; } 
        public int RequestedQuantity { get; private set;}
        public int TotalQuantity { get; private set;}
        public int TotalPages { get; private set;}
        //posible maximum quantity of products per page, can't be over totalQuantity
        public int PossibleMaxQuantity {  get; private set;}

        //quantity of products that needs to be skipped before those products that has to be displayed on the requested page
        public int QuantityToSkip { get; private set;}
    }
}
