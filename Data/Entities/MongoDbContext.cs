using MongoDB.Driver;

namespace WebApplicationZyfra.Data.Entities
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient mongoClient)
        {
            _database = mongoClient.GetDatabase("MyAppDb"); // Название базы данных
        }

        // Коллекция для работы с сессиями
        public IMongoCollection<UserSession> UserSessions => _database.GetCollection<UserSession>("UserSessions");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

    }
}
