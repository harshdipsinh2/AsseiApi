using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface IRoleMasterRepository
    {
        Task<IEnumerable<RoleMaster>> GetRolesAsync();
        Task<RoleMaster> GetRoleByIdAsync(int roleId);
        Task AddRoleAsync(RoleMaster role);
        Task<bool> DeleteRoleAsync(int roleId);

    }

}
