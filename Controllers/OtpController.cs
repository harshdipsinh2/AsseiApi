using AssetManagement.Interfaces;
using Microsoft.AspNetCore.Mvc;
using AssetManagement.Models;

namespace AssetManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailRepository _emailRepository;

        public OtpController(IOtpRepository otpRepository, IUserRepository userRepository, IEmailRepository emailRepository)
        {
            _otpRepository = otpRepository;
            _userRepository = userRepository;
            _emailRepository = emailRepository;
        }

        // Forgot Password: Generate OTP and send it via email
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Email not found." });
            }

            // Generate OTP for Password Reset (OTPType = 1)
            var otp = await _otpRepository.GenerateOtp(user.Username, 1);

            // Send OTP email
            await _emailRepository.SendOtpEmailAsync(user.Email, otp.OTPCode);

            return Ok(new { message = "OTP sent to your email." });
        }


        // Reset Password: Verify OTP and update password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            // Verify OTP for Password Reset (OTPType = 1)
            var otpValid = await _otpRepository.VerifyOtp(request.Username, request.OtpCode, 1);
            if (!otpValid)
            {
                return BadRequest(new { message = "Invalid or expired OTP." });
            }

            // Update the user's password
            var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateUserAsync(user);

            return Ok(new { message = "Password reset successfully." });
        }

        [HttpPost("verify-login-otp")]
        public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyOtpRequest request)
        {
            var otpValid = await _otpRepository.VerifyOtp(request.Username, request.OtpCode, 2);
            if (!otpValid)
            {
                return BadRequest(new { message = "Invalid or expired OTP." });
            }

            // Get user details to generate a token
            var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            // Generate JWT token after successful OTP verification
            var token = _userRepository.GenerateJwtToken(user);

            return Ok(token);
        }

    }
}
