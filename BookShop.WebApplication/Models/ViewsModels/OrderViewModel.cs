namespace BookShop.WebApplication.Models.ViewsModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; } = new();

        public IEnumerable<Order> Orders { get; set; } = [];
    }
}
