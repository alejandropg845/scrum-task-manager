using System.Net.Mail;
using System.Net;
using TaskManager.Users.Interfaces;

namespace TaskManager.Users.Services
{
    public class MailRecoveryPasswordService : IMailRecoveryPasswordService
    {
        private readonly string _email;
        private readonly string _password;
        public MailRecoveryPasswordService(IConfiguration configuration)
        {
            _email = configuration["Credentials:Email"]!;
            _password = configuration["Credentials:Password"]!;
        }
        public async Task SendCodeToEmailAsync(string email, string message)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("email@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Reset password request";
                mail.Body = $"<h1>{message}</h1>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(_email, _password);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
        }
    }
}
