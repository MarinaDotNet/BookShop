namespace BookShop.WebApplication.Models.ViewsModels
{
    public class PageViewModel 
    {
        private int _totalPages = 0;
        public int currentPage { get; set; } = 1;
        public int quantityPerPage { get; set; } = 6;
        public void SetTotalPages (int totalQuantity)
        {
            _totalPages = totalQuantity % quantityPerPage == 0 ?
                    totalQuantity / quantityPerPage :
                    (totalQuantity / quantityPerPage) + 1;
        }

        public int GetTotalPages() { return _totalPages; }

    }
}
