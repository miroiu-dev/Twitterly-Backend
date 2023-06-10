using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Twitter.Dtos;

namespace Twitter.Services
{
    public class EmailService : IEmailService
    {
        private readonly TransactionalEmailsApi _api;
        public EmailService(IConfiguration config)
        {
            var apiKey = config.GetValue<string>("Secrets:Email:ApiKey");
            var apiConfig = new Configuration();

            apiConfig.ApiKey.Add("api-key", apiKey);

            _api = new TransactionalEmailsApi(apiConfig);
        }

        public async void SendEmailAsync(EmailDto email, long? templateId)
        {

            SendSmtpEmailSender sender = new(email.From.Name, email.From.Email);
            SendSmtpEmailTo reciever = new(email.To.Email, email.To.Name);
            List<SendSmtpEmailTo> recievers = new() { reciever };

            var sendSmtpEmail = new SendSmtpEmail(sender, recievers, templateId: templateId, _params: email.Params);
            await _api.SendTransacEmailAsync(sendSmtpEmail);
        }

    }
}
