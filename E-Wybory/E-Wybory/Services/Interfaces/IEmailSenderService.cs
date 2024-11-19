using Azure.Communication.Email;

namespace E_Wybory.Services.Interfaces
{
    public interface IEmailSenderService
    {
        public Task<EmailSendOperation> SendEmailAsync(string email, string subject, string message);
    }
}
