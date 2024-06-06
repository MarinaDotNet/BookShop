using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
/*
 * Entity Model class for storing result from API
 * 
 */
namespace BookShop.WebApplication.Models
{
    public class Product
    {
        public string? Id { get; set; }

        public string? Annotation { get; set; }

        public string? Author { get; set; }

        public bool IsAvailable { get; set; }

        public string? Title { get; set; }

        public string[] Genres { get; set; } = ["undefined"];

        public string? Language { get; set; }

        public Uri Link { get; set; } = new Uri("about:blank");

        [Range(0, int.MaxValue)]
        [Precision(18, 2)]
        public decimal Price { get; set; } = 0;

        [Range(1, int.MaxValue)]
        public int QuantityInOrder { get; set; } = 1;
    }
}
