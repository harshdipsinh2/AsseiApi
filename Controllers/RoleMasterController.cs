using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleMasterController : ControllerBase
    {
        private readonly IRoleMasterRepository _roleMasterRepository;

        public RoleMasterController(IRoleMasterRepository roleMasterRepository)
        {
            _roleMasterRepository = roleMasterRepository;
        }

        [HttpGet]
        [Authorize(Roles = "1,2,3")] 
        public async Task<ActionResult<IEnumerable<RoleMaster>>> GetRoles()
        {
            var roles = await _roleMasterRepository.GetRolesAsync();
            return Ok(new { message = "Roles retrieved successfully.", data = roles });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<RoleMaster>> GetRole(int id)
        {
            var role = await _roleMasterRepository.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found." });
            }
            return Ok(new { message = "Role retrieved successfully.", data = role });
        }

        [HttpPost]
        [Authorize(Roles = "1")] // Only Super Admin should add roles
        public async Task<ActionResult> AddRole(RoleMaster role)
        {
            await _roleMasterRepository.AddRoleAsync(role);
            return CreatedAtAction(nameof(GetRole), new { id = role.RoleID }, new { message = "Role created successfully.", data = role });
        }

        [HttpDelete("{roleId}")]
        [Authorize(Roles = "1")] // Only Super Admin should delete roles
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            try
            {
                bool deleted = await _roleMasterRepository.DeleteRoleAsync(roleId);
                if (!deleted)
                    return NotFound(new { message = "Role not found." });

                return Ok(new { message = "Role deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }

}
