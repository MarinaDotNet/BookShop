/*
 * Entity Model class for Monogo database Books Collection
 */
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookShop.API.Models
{
    public class Product
    {
        [BsonId]
        [ReadOnly(true)]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string ? Id { get; set; }

        [BsonElement("annotation")]
        public string ? Annotation { get; set; }

        [BsonElement("author")]
        public string ? Author { get; set; }

        [BsonElement("available")]
        public bool IsAvailable { get; set; }

        [BsonElement("book")]
        public string ? Title { get; set; }

        [BsonElement("genre")]
        public string[] Genres { get; set; } = ["undefined"];

        [BsonElement("language")]
        public string ? Language { get; set; }

        [BsonElement("link")]
        public Uri Link { get; set; } = new Uri("about:blank");

        [BsonElement("price")]
        [Range(0, int.MaxValue)]
        [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
        public decimal Price { get; set; } = 0;
    }
}
