using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using SocialMediaApp.Models;
using SocialMediaApp.Models.Shared;

namespace SocialMediaApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailModel emailModel);
    }
    public class EmailService(ILogger<EmailService> logger, ISendGridClient sendGridClient, IOptions<AppSettings> appSettings) : IEmailService
    {
        public async Task SendEmailAsync(EmailModel emailModel)
        {
            try
            {
                var msg = new SendGridMessage()
                {
                    Subject = emailModel.Subject,
                    From = new EmailAddress(appSettings.Value.EmailSettings.FromEmail, appSettings.Value.EmailSettings.FromName),
                    TemplateId = emailModel.TemplateId,
                    ReplyTo = new EmailAddress(appSettings.Value.EmailSettings.FromEmail, appSettings.Value.EmailSettings.FromName),
                };
                msg.SetTemplateData(emailModel.DynamicValues);
                msg.SetTemplateId(emailModel.TemplateId);
                msg.AddTo(new EmailAddress(emailModel.ToAddress));
                await sendGridClient.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred in sending email. {error}", JsonConvert.SerializeObject(ex));
            }
        }
    }
}