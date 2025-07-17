using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeePhysicalAssetController : ControllerBase
    {
        private readonly IEmployeePhysicalAssetRepository _assetRepository;

        public EmployeePhysicalAssetController(IEmployeePhysicalAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        private int GetCurrentUserCompanyId()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyID")?.Value;
            return companyIdClaim != null ? int.Parse(companyIdClaim) : 0;
        }

        // GET: api/EmployeePhysicalAsset/all
        [HttpGet("all")]
        [Authorize(Roles = "1,2,3")]
        public async Task<IActionResult> GetEmployeePhysicalAssetsAsync()
        {
            var companyId = GetCurrentUserCompanyId();
            var assignments = await _assetRepository.GetEmployeePhysicalAssetsAsync();

            if (assignments == null || !assignments.Any())
            {
                return NotFound(new { message = "No assignments found." });
            }

            var filteredAssignments = assignments.Where(a => a.CompanyID == companyId).ToList();



            return Ok(new { message = "Employee physical assets retrieved successfully.", data = filteredAssignments });
        }

        // GET: api/EmployeePhysicalAsset/{employeeId}
        [HttpGet("{employeeId}")]
        [Authorize(Roles = "1,2,3")]
        public async Task<IActionResult> GetEmployeeAssets(int employeeId)
        {
            var companyId = GetCurrentUserCompanyId();
            var assignedAssets = await _assetRepository.GetEmployeePhysicalAssetsByEmployeeIdAsync(employeeId);



            var filteredAssets = assignedAssets.Where(a => a.CompanyID == companyId).ToList();



            return Ok(new { message = "Employee assets retrieved successfully.", data = filteredAssets });
        }

        // POST: api/EmployeePhysicalAsset/assign-multiple
        [HttpPost("assign-multiple")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AssignMultiplePhysicalAssetsToEmployeeAsync([FromBody] AssignMultipleAssetsDto request)
        {
            if (request == null || request.EmployeeId <= 0 || request.AssetIds == null || !request.AssetIds.Any())
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            DateTime assignedDate = request.AssignedDate != default ? request.AssignedDate : DateTime.Now;
            int companyId = GetCurrentUserCompanyId(); // 👈 Get CompanyID from claims

            try
            {
                var result = await _assetRepository.AssignMultiplePhysicalAssetsToEmployeeAsync(
                    request.EmployeeId,
                    request.AssetIds,
                    assignedDate,
                    companyId // 👈 Pass CompanyID to repository
                );

                if (!string.IsNullOrEmpty(result) && result.Contains("success", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { message = result });
                }
                else
                {
                    return BadRequest(new { message = result ?? "Failed to assign assets." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }


        // POST: api/EmployeePhysicalAsset/transfer
        [HttpPost("transfer")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> TransferPhysicalAsset([FromBody] TransferAssetDto request)
        {
            if (request == null || request.OldEmployeeId <= 0 || request.NewEmployeeId <= 0 || request.AssetId <= 0)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            var result = await _assetRepository.TransferPhysicalAssetAsync(request.OldEmployeeId, request.NewEmployeeId, request.AssetId);

            if (result.Contains("success", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new { message = result });
            }
            else
            {
                return BadRequest(new { message = result });
            }
        }

        // DELETE: api/EmployeePhysicalAsset/DeletePhysicalAssetAssignment/{employeeId}/{assetId}
        [HttpDelete("DeletePhysicalAssetAssignment/{employeeId}/{assetId}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteEmployeePhysicalAsset(int employeeId, int assetId)
        {
            try
            {
                var result = await _assetRepository.DeleteEmployeePhysicalAssetAsync(employeeId, assetId);
                if (result)
                {
                    return Ok(new { message = "Asset unassigned successfully." });
                }
                else
                {
                    return NotFound(new { message = "Assignment not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }
    }
}
