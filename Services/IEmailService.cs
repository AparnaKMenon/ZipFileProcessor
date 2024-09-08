using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipFileProcessor.Services
{
    internal interface IEmailService
    {
        public Task SendEmailAsync(string subject, string body);
        public void SendEmail(string subject, string body);
    }
}
