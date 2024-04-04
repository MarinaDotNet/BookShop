/*
 * 
 * Performs database operations
 * Creates a new database with collection and copies data from a read-only database.
 * 
 * In each name of created database adds date and time it has been created and serial index number
 * 
 * Because this API maybe accessed by different users(manipulation to Collection)
 * So every time this API starts it creates the collection that user of API will interacts with
 * And copies every time data from not editable Collection to the Collecion for user API
 * Just to avoid possible issues like: empty Collection or to avoid data entered (for example profanity) 
 * by a previous user
 * 
 * !ATTENTION! Could be error: in case if Cluster over size or multiple users uses at same time
 * 
 */
using BookShop.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookShop.API.Controllers.Services
{
    public class StockDBServices
    {
        private readonly IMongoCollection<Product> _collection;

        public StockDBServices(IOptions<ProductDatabaseSettings> settings)
        {
            try
            {
                var mongoClient = new MongoClient(settings.Value.Connection);
                var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);

                //TODO find different way to name databases, ideally to use userID
                //getting the current list of all databases
                List<string> listDatabases = mongoClient.ListDatabaseNames().ToList().Where(name => name.StartsWith(settings.Value.DatabaseName + "_")).ToList();

                //creating new database name or getting previus if has
                string databaseName = GetSetDatabaseName(listDatabases, settings.Value.DatabaseName);

                var userDatabase = mongoClient.GetDatabase(databaseName);
                if (!userDatabase.ListCollectionNames().Any())
                {
                    //getting data from collection to copy into new database and collection
                    var content = mongoDatabase.GetCollection<Product>(settings.Value.CollectionName);
                    userDatabase.CreateCollection(settings.Value.CollectionName);
                    userDatabase.GetCollection<Product>(settings.Value.CollectionName).InsertMany(content.Find(_ => true).ToList());
                }

                _collection = userDatabase.GetCollection<Product>(settings.Value.CollectionName);
            }
            catch (Exception ex)
            {
                _collection = null!;
                Console.WriteLine(ex.ToString());
            }
            
        }
        #region of Help Methods
        private static string GetSetDatabaseName(List<string> listDatabases, string mainDatabase)
        {
            string databaseName = "";

            //getting last database name
            string lastDatabaseName = listDatabases.Count > 0 ?
                listDatabases.ElementAt(listDatabases.Count - 1) : string.Empty;

            if (File.Exists("databaseName.txt")) 
            {
                using StreamReader reader = new("databaseName.txt");
                databaseName = reader.ReadLine()!;
                reader.Close();

                //checking if text file with name of database not empty or if database with name from text file already been deleted
                if (string.IsNullOrEmpty(databaseName) || !listDatabases.Contains(databaseName))
                {
                    databaseName = mainDatabase + '_' + GetDateTimeNow() + "_" + GetDatabaseSerialNumber(lastDatabaseName);
                    SaveDatabaseName(databaseName);
                }
            }
            else
            {
                databaseName = mainDatabase + '_' + GetDateTimeNow() + '_' + GetDatabaseSerialNumber(lastDatabaseName);
                SaveDatabaseName(databaseName);
            }

            return databaseName;
        }

        //Saves new or updates old database name for each user, in databaseName.txt
        private static void SaveDatabaseName(string databaseName)
        {
            using StreamWriter writer = new("databaseName.txt");
            writer.WriteLine(databaseName);
            writer.Close();
        }

        //gets last serial number of exists databse  and increments it by 1
        //if there nor databases with serial numbers, then just returns 1
        private static int GetDatabaseSerialNumber(string databaseName)
        {
            //int number = 1;
            //if(string.IsNullOrEmpty(databaseName))
            //{
            //    return number;
            //}
            //if(databaseName.Contains("PM") || databaseName.Contains("AM")) 
            //{
            //    number += int.Parse(databaseName!.Split('_').ElementAt(4));
            //}
            //else
            //{
            //    number += int.Parse(databaseName!.Split('_').ElementAt(3));
            //}
            //return number;

            int number = string.IsNullOrEmpty(databaseName) ?
                1 :
                int.Parse(databaseName.Split('_').ElementAt(
                    databaseName.Contains("PM") || databaseName.Contains("AM") ? 4 : 3)) + 1;

            return number;
        }

        //return current date and time to add it to new name of database
        private static string GetDateTimeNow()
        {
            string dateTime = DateTime.Now.ToString().Replace(' ', '_');
            dateTime = dateTime.Replace('/', '-');
            dateTime = dateTime.Replace(':', '-');
            dateTime = dateTime.Replace('.', '-');
            return dateTime;
        }
        #endregion

        #region of retrive data from DB Collection
        public async Task<List<Product>> GetAllBooksAsync() =>
           await _collection.Find(_ => true).ToListAsync();

        public async Task<Product> GetBookByIdAsync(string id) =>
            await _collection.Find(_ => _.Id!.Equals(id)).FirstOrDefaultAsync();

        //returns list of books where Title,Author, Language or one of item from array of Genres EQUALS to searchCondition
        public async Task<List<Product>> GetBooksEqualsConditionAsync(string condition) =>
            await _collection.Find(_ => 
            _.Author!.Equals(condition, StringComparison.OrdinalIgnoreCase) ||
            _.Title!.Equals(condition, StringComparison.OrdinalIgnoreCase) ||
            _.Language!.Equals(condition, StringComparison.OrdinalIgnoreCase) ||
            _.Genres!.ToArray().Any(genre => genre.Equals(condition, StringComparison.OrdinalIgnoreCase))
            ).ToListAsync();

        //returns list of books where Title,Author, Language or one of item from array of genres CONTAINS to searchCondition
        public async Task<List<Product>> GetBooksContainsConditionAsync(string condition) =>
            await _collection.Find(_ =>
            _.Author!.ToUpper().Contains(condition.ToUpper()) ||
            _.Title!.ToUpper().Contains(condition.ToUpper()) ||
            _.Language!.ToUpper().Contains(condition.ToUpper()) ||
            _.Genres!.ToArray().Any(genre => genre.ToUpper().Contains(condition.ToUpper()))
            ).ToListAsync();
        #endregion
        #region of manipulations with data from DB Collection
        public async Task AddNewAsync(Product product) =>
            await _collection.InsertOneAsync(product);

        public async Task UpdateNewAsync(Product product) =>
            await _collection.FindOneAndReplaceAsync(book => book.Id!.Equals(product.Id), product);

        public async Task DeleteOneAsync(string id) =>
            await _collection.DeleteOneAsync(id);
        #endregion
    }
}
