using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplicationZyfra.Data.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        public string Login { get; set; }

        [BsonIgnore]  // Игнорируем это поле при сериализации
        public string Password { get; set; }  // Обычный пароль (не храним в базе данных)
        public string PasswordHash { get; set; }  // Хеш пароля
        public string Salt { get; set; }
    }
}
