namespace SocialMediaApp.Models
{
    public class AppSettings
    {
        public bool IsDevelopment { get; set; } = false;
        public string FrontendUrl { get; set; }
        public string DefaultUserRole { get; set; }
        public Jwt Jwt { get; set; }
        public MongoDBSettings MongoDBSettings { get; set; }
        public EmailSettings EmailSettings { get; set; }
    }

    public class MongoDBSettings
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
    }

    public class Jwt
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
        public int ExpiryInMinutes { get; set; }
        public int RefreshTokenExpiryInDays { get; set; }
    }

    public class EmailSettings
    {
        public string ApiKey { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string ConfirmationTemplate { get; set; }
    }

}
