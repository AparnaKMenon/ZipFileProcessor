using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ZipFileProcessor.Services
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string AdminEmail { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly ILoggerService _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(ILoggerService logger, IOptions<EmailSettings> emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string subject, string body)
        {
            try
            {
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(_emailSettings.SenderEmail);
                    mail.To.Add(_emailSettings.AdminEmail);
                    mail.Subject = subject;
                    mail.Body = body;

                    using (var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);

                        // Send the email asynchronously
                        await smtpClient.SendMailAsync(mail);
                    }
                }

                string message = "Email sent successfully.";
                // TODO should probably have information of which file - [applicationno]-[guid]. applicable for all logs
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending email", ex);
            }
        }

        public void SendEmail(string subject, string body)
        {
            try
            {
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(_emailSettings.SenderEmail);
                    mail.To.Add(_emailSettings.AdminEmail);
                    mail.Subject = subject;
                    mail.Body = body;

                    using (var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);

                        // Send the email synchronously
                        smtpClient.Send(mail);
                    }
                }

                string message = "Email sent successfully.";
                // TODO should probably have information of which file - [applicationno]-[guid]. applicable for all logs
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending email", ex);
            }
        }
    }
}
