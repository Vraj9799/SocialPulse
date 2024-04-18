using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialMediaApp.Models.Entities;


public class Activity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonElement("message")]
    [BsonRequired]
    public string Message { get; set; }
    [BsonElement("userId")]
    [BsonRequired]
    public string UserId { get; set; }
    [BsonElement("isRead")]
    public bool IsRead { get; set; }
    [BsonElement("createdOn")]
    public DateTime CreatedOn { get; set; }

    public Activity()
    {
        CreatedOn = DateTime.UtcNow;
        IsRead = false;
    }
}