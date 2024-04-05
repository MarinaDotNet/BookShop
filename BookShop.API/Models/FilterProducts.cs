/*
 * Model to Filter Products by specified query
 * 
 */
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BookShop.API.Models
{
    public class FilterProducts
    {
        public string? Annotation { get; set; }

        public string? Author { get; set; }

        public bool IsAvailable { get; set; }

        public string? Title { get; set; }

        public string[] ? Genres { get; set; }

        public string? Language { get; set; }


        [Range(0, int.MaxValue)]
        public decimal MinPrice { get; set; }

        [Range(0, int.MaxValue)]
        public decimal MaxPrice { get; set; }
    }
}
