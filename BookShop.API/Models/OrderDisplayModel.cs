namespace BookShop.API.Models
{
    public class OrderDisplayModel(Order order, string additionalInformation)
    {
        public string OrderId { get; set; } = order.OrderId;
        public List<string> ProductsId { get; set; } = [..order.ProductsId];
        public decimal TotalPrice { get; set; } = order.TotalPrice;
        public DateTime OrderDateTime { get; set; } = order.OrderDateTime;
        public bool IsSubmitted { get; set; } = order.SubmittedOrder;
        public string Notes { get; set; } = additionalInformation;
    }
}
