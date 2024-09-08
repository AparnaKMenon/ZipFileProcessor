namespace ZipFileProcessor.Services
{
    internal interface IEmailService
    {
        public Task SendEmailAsync(string subject, string body);
        public void SendEmail(string subject, string body);
    }
}
