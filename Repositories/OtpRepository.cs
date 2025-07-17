using Microsoft.EntityFrameworkCore;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using AssetManagement.Data;
using System.Security.Cryptography;

namespace AssetManagement.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly AppDbContext _context;

        public OtpRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Otp> GenerateOtp(string username, int otpType)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty.");
            }

            int otpValue = RandomNumberGenerator.GetInt32(100000, 999999);
            string otpCode = otpValue.ToString();

            var user = await _context.tb_Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var otp = new Otp
            {
                Username = user.Username,
                OTPCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                OTPType = otpType // Assign OTPType
            };

            _context.Otp.Add(otp);
            await _context.SaveChangesAsync();

            return otp;
        }


        public async Task<bool> VerifyOtp(string username, string otpCode, int otpType)
        {
            var otp = await _context.Otp
                .Where(o => o.Username == username && o.OTPCode == otpCode && !o.IsUsed && o.OTPType == otpType)
                .OrderByDescending(o => o.ExpiryTime)
                .FirstOrDefaultAsync();

            if (otp == null || otp.ExpiryTime < DateTime.UtcNow)
            {
                return false; // OTP expired or invalid
            }

            otp.IsUsed = true; // Mark OTP as used
            await _context.SaveChangesAsync();
            return true;
        }


        // Implement GetUserByEmailAsync method to find a user by email
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.tb_Users
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        // Implement UpdatePasswordAsync method to update the user's password using Username
        public async Task<bool> UpdatePasswordAsync(string username, string newPassword)
        {
            var user = await _context.tb_Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return false; // User not found
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.tb_Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
