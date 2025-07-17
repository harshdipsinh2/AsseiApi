using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync(int companyId);
        Task<User> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username); // Add this line
        Task AddUserAsync(User user, int companyId);
        Task<User> AuthenticateUserAsync(string username, string password);
        Task<User?> GetUserProfileAsync(string username);
        Task<bool> DeleteUserByEmailAsync(string email);
        string GenerateJwtToken(User user);

        Task UpdateUserAsync(User user);
    }
}

