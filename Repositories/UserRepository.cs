using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AssetManagement.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(int companyId)
        {
            var users = await _context.tb_Users
                .Include(u => u.Employee)
                .Where(u => u.CompanyID == companyId)
                .Select(u => new UserDTO
                {
                    UserID = u.UserID,
                    Username = u.Username,
                    RoleID = u.RoleID,
                    EmployeeID = u.EmployeeID != null ? u.Employee.EmployeeId : 0,
                    Name = u.Employee != null ? u.Employee.EmployeeName : "N/A",
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber
                })
                .ToListAsync();

            return users;
        }


        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.tb_Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.tb_Users.FindAsync(userId);
        }

        public async Task AddUserAsync(User user, int companyId)
        {
            var existingUser = await _context.tb_Users
                .FirstOrDefaultAsync(u => u.Username == user.Username);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Username is already taken.");
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                throw new ArgumentException("Password cannot be null or empty.");
            }

            // Hash password before saving
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = null;

            // Ensure Email and PhoneNumber are provided
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PhoneNumber))
            {
                throw new ArgumentException("Email and PhoneNumber are required.");
            }


            user.CompanyID = companyId;
            _context.tb_Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.tb_Users.Include(u => u.Role)
                                              .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }

            return user;
        }

        public async Task<User?> GetUserProfileAsync(string username)
        {
            return await _context.tb_Users
                .Include(u => u.Role) // Include Role if needed
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> DeleteUserByEmailAsync(string email)
        {
            var user = await _context.tb_Users.FirstOrDefaultAsync(u => u.Email == email);  // Changed to Email
            if (user == null)
            {
                return false;
            }

            // Delete OTP records related to this user
            await _context.Otp.Where(o => o.Username == user.Username).ExecuteDeleteAsync();

            // Now delete the user
            _context.tb_Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("EmployeeID", user.EmployeeID.ToString()),
                new Claim(ClaimTypes.Role, user.RoleID.ToString()),
                new Claim("RoleId", user.RoleID.ToString()), //  Explicit custom claim
                new Claim("CompanyID", user.CompanyID.ToString())
            };


            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.tb_Users
                .FirstOrDefaultAsync(u => u.Email == email); // Fetch by email instead of EmployeeID
        }

        public async Task<User?> GetUserByEmployeeIdAsync(int employeeId)
        {
            return await _context.tb_Users
                .FirstOrDefaultAsync(u => u.EmployeeID == employeeId);  // You can keep this method if you need it, but it's now replaced by Email
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _context.tb_Users.FindAsync(user.UserID);
            if (existingUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Update User fields
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Email = user.Email;

            // Also update Employee if linked
            var employee = await _context.tb_Employees.FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeID);
            if (employee != null)
            {
                employee.PhoneNumber = user.PhoneNumber;
                employee.EmailId = user.Email;
            }

            await _context.SaveChangesAsync();
        }
    }
}