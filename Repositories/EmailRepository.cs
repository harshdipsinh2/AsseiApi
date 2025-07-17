using System.Net;
using System.Net.Mail;
using AssetManagement.Interfaces;


namespace AssetManagement.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587; 
        private readonly string _smtpUser = "pulikrpatel@gmail.com"; 
        private readonly string _smtpPassword = "fwlvsmicvnqtikgz";  

        // Send email with OTP
        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPassword);
                smtpClient.EnableSsl = true; // Enable SSL for security

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = "Your OTP Code",
                    Body = $"<p>Your OTP code is: <strong>{otpCode}</strong></p>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
