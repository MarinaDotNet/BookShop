/*
 * Entity Model class for the SQL table Orders
 */
using BookShop.API.Models.Authentication;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.API.Models
{
    [Table("Orders")]
    public class Order
    {
        [ForeignKey("UserId")]
        public string UserId { get; set; } = string.Empty!;

        public string OrderId { get; set; } = string.Empty!;

        public ApiUser User { get; set; } = new();
        public List<string>? ProductsId { get; set; } = new();

        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }

        public DateTime OrderDateTime { get; set; }

        public bool SubmittedOrder { get; set; }
    }
}
