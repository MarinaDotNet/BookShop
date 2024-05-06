/*
 * View Model
 */
namespace BookShop.WebApplication.Models.ViewsModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; } = new();

        public IEnumerable<Product> Products { get; set; } = [];
    }
}
