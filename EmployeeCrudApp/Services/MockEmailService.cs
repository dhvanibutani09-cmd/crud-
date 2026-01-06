
namespace EmployeeCrudApp.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly IWebHostEnvironment _env;

        public MockEmailService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var logPath = Path.Combine(_env.ContentRootPath, "email_log.txt");
            var logEntry = $"[{DateTime.Now}] To: {to} | Subject: {subject} | Body: {body}{Environment.NewLine}";
            await File.AppendAllTextAsync(logPath, logEntry);
        }
    }
}
