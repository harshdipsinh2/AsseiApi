using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models
{
    [Table("tb_OTP")]
    public class Otp
    {
        public int OTPID { get; set; }
        public string Username { get; set; }
        public string OTPCode { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; }
        public int OTPType { get; set; }

    }

    // Request class for Forgot Password
    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    // Request class for Reset Password
    public class ResetPasswordRequest
    {
        public string Username { get; set; }
        public string OtpCode { get; set; }
        public string NewPassword { get; set; }
    }

    // Add this class for Login OTP Verification
    public class VerifyOtpRequest
    {
        public string Username { get; set; }
        public string OtpCode { get; set; }
    }
}
