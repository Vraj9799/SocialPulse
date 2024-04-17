using MongoDB.Bson.Serialization.Attributes;

namespace SocialMediaApp.Models.Entities
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("postId")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string PostId { get; set; }

        public DateTime CreatedOn { get; set; }

        [BsonElement("lastModifiedOn")]
        public DateTime LastModifiedOn { get; set; }

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }

        public Comment()
        {
            CreatedOn = DateTime.UtcNow;
            LastModifiedOn = DateTime.UtcNow;
            IsDeleted = false;
        }
    }
}
