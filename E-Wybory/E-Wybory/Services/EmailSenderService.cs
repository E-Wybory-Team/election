using Azure;
using Azure.Communication.Email;
using E_Wybory.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace E_Wybory.Services
{
    public class EmailSenderService : IEmailSenderService
    {

        private readonly EmailOptions _emailOptions;

        public EmailSenderService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public async Task<EmailSendOperation> SendEmailAsync(string email, string subject, string message)
        {
            
            var emailClient = new EmailClient(_emailOptions.ConnectionString);


            var emailMessage = new EmailMessage(
                senderAddress: _emailOptions.SenderAddress,
                content: new EmailContent(subject)
                {
                    PlainText = message,
                    Html = $@"
		            <html>
			            <body>
				            <h1>{message}</h1>
			            </body>
		            </html>"
                },
                recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(email) }));


            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage);

            return emailSendOperation;
        }

        private int getUserIdFromFileName(string fileName)
        {

            int underscoreIndex = fileName.LastIndexOf('_');
            int dotIndex = fileName.LastIndexOf('.');

            string numberPart = fileName.Substring(underscoreIndex + 1, dotIndex - underscoreIndex - 1);
            int number = int.Parse(numberPart);

            //Console.WriteLine($"Extracted number: {number}");
            return number;
        }

        public async Task<EmailSendOperation> SendEmailWithPdfAttachmentAsync(string email, string subject, string message, string pdfFilePath)
        {
            BinaryData pdfContent = BinaryData.FromBytes(await File.ReadAllBytesAsync(pdfFilePath));
            var userId = getUserIdFromFileName(pdfFilePath);

            EmailAttachment pdfAttachment = new EmailAttachment(
                name: $"generatedDocument_{userId}.pdf",
                contentType: "application/pdf", 
                content: pdfContent 
            );

            var emailClient = new EmailClient(_emailOptions.ConnectionString);

            var emailMessage = new EmailMessage(
                senderAddress: _emailOptions.SenderAddress,
                content: new EmailContent(subject)
                {
                    PlainText = message,
                    Html = $@"
		            <html>
			            <body>
				            <h1>{message}</h1>
			            </body>
		            </html>"
                },
                recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(email) }));

            emailMessage.Attachments.Add(pdfAttachment);

            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage);

            return emailSendOperation;
        }
    }

    public class EmailOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string SenderAddress { get; set; } = null!;
    }
    public static class BuilderExtensionMethods
    {

        public static WebApplicationBuilder ConfigureEmailService(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSenderService, EmailSenderService>();
            return builder;
        }
    }
}
