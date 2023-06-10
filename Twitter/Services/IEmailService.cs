using Twitter.Dtos;

namespace Twitter.Services
{
    public interface IEmailService
    {
        void SendEmailAsync(EmailDto email, long? templateId);
    }
}
