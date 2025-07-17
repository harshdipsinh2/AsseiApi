
namespace AssetManagement.Interfaces
{
    public interface IEmailRepository
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);  // Send OTP email
    }
}
