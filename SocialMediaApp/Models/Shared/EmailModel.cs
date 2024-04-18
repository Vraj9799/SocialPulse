using SocialMediaApp.Models.Entities;

namespace SocialMediaApp.Models.Shared
{
    public class EmailModel
    {
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, string> DynamicValues { get; set; }
    }
}