using AssetManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Interfaces
{
    public interface IOtpRepository
    {
        Task<Otp> GenerateOtp(string username, int otpType);  // Changed from employeeId to username
        Task<bool> VerifyOtp(string username, string otpCode, int otpType);  // Changed from employeeId to username
        Task<User> GetUserByEmailAsync(string email); // No change needed
        Task<bool> UpdatePasswordAsync(string username, string newPassword);  // Changed from employeeId to username
    }
}
