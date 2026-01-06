using System.Net;
using System.Net.Mail;

namespace EmployeeCrudApp.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // For testing purposes, we'll try to get settings from appsettings.json
            // If not provided, we will fall back to the MockEmailService behavior or log an error.
            
            var smtpHost = _configuration["EmailSettings:Host"];
            var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            var smtpUser = _configuration["EmailSettings:Username"];
            var smtpPass = _configuration["EmailSettings:Password"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
            {
                // Fallback to logging for now if settings are missing
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "email_log.txt");
                var logEntry = $"[{DateTime.Now}] (SMTP MISSING SETTINGS) To: {to} | Subject: {subject} | Body: {body}{Environment.NewLine}";
                await File.AppendAllTextAsync(logPath, logEntry);
                return;
            }

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Fallback to logging if SMTP fails (e.g. SocketException, SmtpException)
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "email_log.txt");
                var logEntry = $"[{DateTime.Now}] (SMTP FAILED: {ex.Message}) To: {to} | Subject: {subject} | Body: {body}{Environment.NewLine}";
                await File.AppendAllTextAsync(logPath, logEntry);
            }
        }
    }
}
