using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailRepository _emailRepository;

        public UserController(AppDbContext context, IUserRepository userRepository, IOtpRepository otpRepository, IEmailRepository emailRepository)
        //public UserController(AppDbContext context, IUserRepository userRepository)

        {
            _context = context;
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _emailRepository = emailRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Check if Username already exists
            var existingUser = await _context.tb_Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Username is already taken." });
            }

            //  If both EmployeeID and RoleID are null, set RoleID = 1
            if (user.EmployeeID == null && user.RoleID == null)
            {
                user.RoleID = 1;
            }

            //  Prevent duplicate user accounts for the same EmployeeID
            if (user.EmployeeID != null)
            {
                var existingEmployeeUser = await _context.tb_Users
                    .FirstOrDefaultAsync(u => u.EmployeeID == user.EmployeeID);
                if (existingEmployeeUser != null)
                {
                    return BadRequest(new { message = "An account already exists for this Employee ID." });
                }
            }


            //  Only check employee validation if EmployeeID is provided
            if (user.EmployeeID != null)
            {
                var employee = await _context.tb_Employees.FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeID);
                if (employee == null)
                {
                    return BadRequest(new { message = "EmployeeID is invalid or does not exist." });
                }

            // Validate RoleID against employee's assigned RoleID
                if (user.RoleID != employee.RoleID)
                {
                    return BadRequest(new { message = "You are not allowed to register with this RoleID." });
                }
            }

            // Prevent duplicate Super Admins for a company
            if (user.RoleID == 1)
            {
                var existingSuperAdmin = await _context.tb_Users
                    .FirstOrDefaultAsync(u => u.RoleID == 1 && u.CompanyID == user.CompanyID);
                if (existingSuperAdmin != null)
                {
                    return BadRequest(new { message = "This company already has a Super Admin." });
                }
            }

            // Hash the password
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // Avoid EF navigation property issues
            user.Role = null;

            _context.tb_Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully!" });
        }




        [HttpPost("add-user")]
        [Authorize(Roles = "1")] // Only Super Admin can access this endpoint
        public async Task<IActionResult> AddNewUser([FromBody] User newUser)
        {
            // Check if Username already exists
            var existingUser = await _context.tb_Users.FirstOrDefaultAsync(u => u.Username == newUser.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Username is already taken." });
            }

            // Check if Email already exists
            var existingEmailUser = await _context.tb_Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
            if (existingEmailUser != null)
            {
                return BadRequest(new { message = "Email is already taken." });
            }

            // Hash the password
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            // Prevent duplicate user accounts for the same EmployeeID
            if (newUser.EmployeeID != null)
            {
                var existingEmployeeUser = await _context.tb_Users
                    .FirstOrDefaultAsync(u => u.EmployeeID == newUser.EmployeeID);
                if (existingEmployeeUser != null)
                {
                    return BadRequest(new { message = "An account already exists for this Employee ID." });
                }
            }


            // Set default RoleId to 3 (Employee) if not provided by Super Admin
            if (newUser.RoleID == null)
            {
                newUser.RoleID = 3; // Default to Employee role if not specified
            }

            // Ensure Role navigation property is null
            newUser.Role = null;

            _context.tb_Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "New user added successfully!" });
        }


        //[HttpPost("login")]
        //[Consumes("application/json")]
        //public async Task<IActionResult> Login([FromBody] LoginRequest user)
        //{
        //    var authenticatedUser = await _userRepository.AuthenticateUserAsync(user.Username, user.Password);
        //    if (authenticatedUser == null)
        //    {
        //        return Unauthorized(new { message = "Invalid username or password." });
        //    }

        //    var token = _userRepository.GenerateJwtToken(authenticatedUser);
        //    return Ok(token);
        //}

        [HttpPost("login")]
        [Consumes("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginRequest user)
        {
            var authenticatedUser = await _userRepository.AuthenticateUserAsync(user.Username, user.Password);

            //  If user is null, return an error before calling OTP generation
            if (authenticatedUser == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            //  Ensure authenticatedUser is not null before generating OTP
            if (authenticatedUser.Username == null)
            {
                return BadRequest(new { message = "User data is incomplete." });
            }

            // Generate OTP for 2FA (OTPType = 2)
            var otp = await _otpRepository.GenerateOtp(authenticatedUser.Username, 2);

            // Send OTP via email (Assuming you have Email Repository)
            await _emailRepository.SendOtpEmailAsync(authenticatedUser.Email, otp.OTPCode);

            return Ok(new { message = "OTP sent to your email. Please verify to continue." });
        }

        private int GetCompanyIdFromToken()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyID");
            if (companyIdClaim == null)
                throw new UnauthorizedAccessException("CompanyID not found in token.");

            return int.Parse(companyIdClaim.Value);
        }


        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            var companyId = GetCompanyIdFromToken();
            var users = await _userRepository.GetAllUsersAsync(companyId);
            return Ok(users);
        }


        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var username = User.Identity.Name;

            var user = await _userRepository.GetUserProfileAsync(username);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                user.UserID,
                user.Username,
                user.EmployeeID,
                user.RoleID,
                user.Email,
                user.PhoneNumber,
                user.CompanyID
            });
        }

        [HttpPut("update-user/{email}")]
        [Authorize(Roles = "1,2,3")] // Super Admin, Admin, Employee
        public async Task<IActionResult> UpdateUser(string email, [FromBody] User updatedUser)
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            var roleIdClaim = User.FindFirst("RoleId")?.Value;

            if (string.IsNullOrEmpty(emailClaim) || string.IsNullOrEmpty(roleIdClaim))
            {
                return Unauthorized(new { message = "Invalid token. Missing email or role." });
            }

            var loggedInEmail = emailClaim;
            var roleId = int.Parse(roleIdClaim);

            var existingUser = await _userRepository.GetUserByEmailAsync(email);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Super Admin can update all fields
            if (roleId == 1)
            {
                existingUser.Username = updatedUser.Username;
                existingUser.Email = updatedUser.Email;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                existingUser.RoleID = updatedUser.RoleID; // Only Super Admin can modify
            }
            else
            {
                // Admins & Employees can update only their own details
                if (loggedInEmail != email)
                {
                    return Forbid(); // 403 
                }

                existingUser.Username = updatedUser.Username;
                existingUser.Email = updatedUser.Email;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                // RoleID stays unchanged
            }

            await _userRepository.UpdateUserAsync(existingUser);
            return Ok(new { message = "User details updated successfully!" });
        }

        [HttpDelete("delete/{email}")]
        [Authorize(Roles = "1,2,3")] // Super Admin, Admin, Employee
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            var roleIdClaim = User.FindFirst("RoleId")?.Value;

            if (string.IsNullOrEmpty(emailClaim) || string.IsNullOrEmpty(roleIdClaim))
            {
                return Unauthorized(new { message = "Invalid token. Missing email or role." });
            }

            var loggedInEmail = emailClaim;
            var roleId = int.Parse(roleIdClaim);

            // Super Admin can delete any user
            if (roleId == 1)
            {
                var success = await _userRepository.DeleteUserByEmailAsync(email);
                if (!success)
                {
                    return NotFound(new { message = "User with given email not found." });
                }
                return Ok(new { message = "User deleted successfully!" });
            }

            // Admin and Employee can delete only themselves
            if (!string.Equals(loggedInEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var userDeleted = await _userRepository.DeleteUserByEmailAsync(email);
            if (!userDeleted)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { message = "Your account has been deleted successfully!" });
        }
    }
}