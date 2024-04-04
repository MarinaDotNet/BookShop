/*
 * Class to store appsettings.json file's ConnectionMongoStock property values
 */
namespace BookShop.API.Models
{
    public class ProductDatabaseSettings
    {
        public string Connection { get; set; } = string.Empty!;
        public string DatabaseName { get; set;} = string.Empty!;
        public string CollectionName { get; set; } = string.Empty!;
    }
}
