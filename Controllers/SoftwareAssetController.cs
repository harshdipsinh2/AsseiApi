using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SoftwareAssetController : ControllerBase
    {
        private readonly ISoftwareAssetRepository _softwareAssetRepository;

        public SoftwareAssetController(ISoftwareAssetRepository softwareAssetRepository)
        {
            _softwareAssetRepository = softwareAssetRepository;
        }

        private int GetCurrentUserCompanyId()
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyID")?.Value;
            return companyIdClaim != null ? int.Parse(companyIdClaim) : 0; // Default to 0 if not found
        }

        // GET: api/SoftwareAsset
        [HttpGet]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<IEnumerable<SoftwareAsset>>> GetSoftwareAssets()
        {
            var companyId = GetCurrentUserCompanyId();
            var softwareAssets = await _softwareAssetRepository.GetAllSoftwareAssetsAsync();
            var filteredAssets = softwareAssets.Where(a => a.CompanyID == companyId).ToList();  // Filter by company ID

            return Ok(new { message = "Software assets retrieved successfully.", data = filteredAssets });
        }

        // GET: api/SoftwareAsset/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<SoftwareAsset>> GetSoftwareAsset(int id)
        {
            var companyId = GetCurrentUserCompanyId();
            var softwareAsset = await _softwareAssetRepository.GetSoftwareAssetByIdAsync(id);

            if (softwareAsset == null || softwareAsset.CompanyID != companyId)
            {
                return NotFound(new { message = "Software asset not found or does not belong to your company." });
            }

            return Ok(new { message = "Software asset retrieved successfully.", data = softwareAsset });
        }

        // POST: api/SoftwareAsset
        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<SoftwareAsset>> PostSoftwareAsset(SoftwareAsset softwareAsset)
        {
            var companyId = GetCurrentUserCompanyId();
            if (softwareAsset.CompanyID != companyId)
            {
                return BadRequest(new { message = "Software asset must belong to your company." });
            }

            await _softwareAssetRepository.AddSoftwareAssetAsync(softwareAsset);
            return CreatedAtAction(nameof(GetSoftwareAsset), new { id = softwareAsset.SoftwareId }, new { message = "Software asset created successfully.", data = softwareAsset });
        }

        // PUT: api/SoftwareAsset/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> PutSoftwareAsset(int id, SoftwareAsset softwareAsset)
        {
            if (id != softwareAsset.SoftwareId)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            var exists = await _softwareAssetRepository.GetSoftwareAssetByIdAsync(id);
            if (exists == null || exists.CompanyID != GetCurrentUserCompanyId())
            {
                return NotFound(new { message = "Software asset not found or does not belong to your company." });
            }

            await _softwareAssetRepository.UpdateSoftwareAssetAsync(softwareAsset);
            return Ok(new { message = "Software asset updated successfully." });
        }

        // DELETE: api/SoftwareAsset/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteSoftwareAsset(int id)
        {
            var softwareAsset = await _softwareAssetRepository.GetSoftwareAssetByIdAsync(id);
            if (softwareAsset == null || softwareAsset.CompanyID != GetCurrentUserCompanyId())
            {
                return NotFound(new { message = "Software asset not found or does not belong to your company." });
            }

            await _softwareAssetRepository.DeleteSoftwareAssetAsync(id);
            return Ok(new { message = "Software asset deleted successfully." });
        }
    }
}
