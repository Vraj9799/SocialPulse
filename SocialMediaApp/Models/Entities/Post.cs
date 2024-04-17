using MongoDB.Bson.Serialization.Attributes;

namespace SocialMediaApp.Models.Entities
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("caption")]
        public string Caption { get; set; }

        [BsonElement("medias")]
        public ISet<string> Medias { get; set; }

        [BsonElement("tags")]
        [BsonIgnoreIfNull]
        public ISet<string> Tags { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public PostStatus Status { get; set; }

        [BsonElement("scheduledOn")]
        [BsonIgnoreIfNull]
        public DateTime? ScheduledOn { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("likedBy")]
        [BsonIgnoreIfNull]
        public ISet<string> LikedBy { get; set; }

        [BsonElement("savedBy")]
        [BsonIgnoreIfNull]
        public ISet<string> SavedBy { get; set; }

        [BsonElement("createdOn")]
        public DateTime CreatedOn { get; set; }

        [BsonElement("lastModifiedOn")]
        public DateTime LastModifiedOn { get; set; }

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }

        public Post()
        {
            CreatedOn = DateTime.UtcNow;
            LastModifiedOn = DateTime.UtcNow;
            IsDeleted = false;
        }
    }
}
