using Azure.Communication.Email;

namespace E_Wybory.Services.Interfaces
{
    public interface IEmailSenderService
    {
        public Task<EmailSendOperation> SendEmailAsync(string email, string subject, string message);
        public Task<EmailSendOperation> SendEmailWithPdfAttachmentAsync(string email, string subject, string message, string pdfFilePath);
    }
}
