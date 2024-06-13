using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.WebApplication.Models
{
    public class Order
    {
        public string ? OrderId { get; set; }

        //public ApplicationUser User { get; set; } = new();
        public string[] ? ProductsId { get; set; } = [];

        public IEnumerable<Product> ? Products { get; set; } = [];

        private decimal totalPrice = 0;
        [Range(0, int.MaxValue)]
        [Precision(18, 2)]
        public decimal TotalPrice 
        {
            get => totalPrice;
            set => totalPrice = Decimal.Parse(value.ToString());
        }

        DateTime orderDateTime;
        public DateTime ? OrderDateTime 
        { 
            get => orderDateTime; 
            set => orderDateTime = DateTime.Parse(value.ToString()!); 
        }

        public bool IsSubmitted { get; set; } = false;

        public string ? Notes {  get; set; }
    }
}
