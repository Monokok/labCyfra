using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace WebApplicationZyfra.Data.Entities
{
    public class UserSession
    {
        
        [BsonId] // id 
        [BsonRepresentation(BsonType.String)]
        public Guid SessionId { get; set; }
        [BsonRepresentation(BsonType.String)]

        public Guid UserId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] // Указывает формат времени
        public DateTime ExpiresAt { get; set; }

    }
}
