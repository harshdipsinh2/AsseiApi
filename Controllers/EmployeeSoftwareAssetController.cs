using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeSoftwareAssetController : ControllerBase
    {
        private readonly IEmployeeSoftwareAssetRepository _repository;

        public EmployeeSoftwareAssetController(IEmployeeSoftwareAssetRepository repository)
        {
            _repository = repository;
        }

        private int GetCurrentUserCompanyId()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyID")?.Value;
            return companyIdClaim != null ? int.Parse(companyIdClaim) : 0;
        }

        // GET: api/EmployeeSoftwareAsset
        [HttpGet]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<IEnumerable<EmployeeSoftwareAsset>>> GetEmployeeSoftwareAssets()
        {
            var companyId = GetCurrentUserCompanyId();
            var allAssignments = await _repository.GetAllAssignmentsAsync();

            var filteredAssignments = allAssignments.Where(a => a.CompanyID == companyId).ToList();


            return Ok(new { message = "Software asset assignments retrieved successfully.", data = filteredAssignments });
        }

        // GET: Get assigned software assets for a specific employee
        [HttpGet("{employeeId}")]
        [Authorize(Roles = "1,2,3")]
        public async Task<IActionResult> GetEmployeeSoftwareAssets(int employeeId)
        {
            var companyId = GetCurrentUserCompanyId();
            var assignedAssets = await _repository.GetEmployeeSoftwareAssetsByEmployeeIdAsync(employeeId);

            var filteredAssets = assignedAssets.Where(a => a.CompanyID == companyId).ToList();



            return Ok(new { message = "Software assets retrieved successfully.", data = filteredAssets });
        }

        // POST: api/EmployeeSoftwareAsset
        [HttpPost]
        [Authorize(Roles = "1")] // Super Admin
        public async Task<ActionResult> AssignMultipleSoftwareAssetsToEmployee(int employeeId, [FromBody] List<int> softwareIds)
        {
            DateTime assignedDate = DateTime.UtcNow;
            int companyId = GetCurrentUserCompanyId();

            var resultMessage = await _repository.AssignMultipleSoftwareAssetsToEmployeeAsync(employeeId, softwareIds, assignedDate, companyId);

            if (resultMessage.Contains("success", StringComparison.OrdinalIgnoreCase))
                return Ok(resultMessage);
            else
                return BadRequest(resultMessage);
        }



        // DELETE: api/EmployeeSoftwareAsset/DeleteSoftwareAssetAssignment/{employeeId}/{softwareId}
        [HttpDelete("DeleteSoftwareAssetAssignment/{employeeId}/{softwareId}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteEmployeeSoftwareAsset(int employeeId, int softwareId)
        {
            try
            {
                var result = await _repository.DeleteEmployeeSoftwareAssetAsync(employeeId, softwareId);
                if (result)
                {
                    return Ok(new { message = "Software successfully unassigned from employee." });
                }
                else
                {
                    return NotFound(new { message = "Assignment not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
