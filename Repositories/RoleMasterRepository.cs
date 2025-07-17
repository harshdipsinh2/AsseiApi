using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.EntityFrameworkCore;


namespace AssetManagement.Repositories
{
    public class RoleMasterRepository : IRoleMasterRepository
    {
        private readonly AppDbContext _context;

        public RoleMasterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoleMaster>> GetRolesAsync()
        {
            return await _context.tb_RoleMaster.ToListAsync();
        }

        public async Task<RoleMaster> GetRoleByIdAsync(int roleId)
        {
            return await _context.tb_RoleMaster.FindAsync(roleId);
        }

        public async Task AddRoleAsync(RoleMaster role)
        {
            _context.tb_RoleMaster.Add(role);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var role = await _context.tb_RoleMaster.FindAsync(roleId);

            if (role == null)
                return false; // Role doesn't exist

            // Check if any users are assigned to this role
            bool isRoleAssigned = await _context.tb_Users.AnyAsync(u => u.RoleID == roleId);
            if (isRoleAssigned)
                throw new InvalidOperationException("Cannot delete a role assigned to users.");

            _context.tb_RoleMaster.Remove(role);
            await _context.SaveChangesAsync();

            return true;
        }

    }

}
