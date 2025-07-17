using Microsoft.AspNetCore.Mvc;
using AssetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using AssetManagement.Interfaces;

namespace AssetManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PhysicalAssetController : ControllerBase
    {
        private readonly IPhysicalAssetRepository _physicalAssetRepository;

        public PhysicalAssetController(IPhysicalAssetRepository physicalAssetRepository)
        {
            _physicalAssetRepository = physicalAssetRepository;
        }

        // GET all Physical Assets:
        [HttpGet]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<IEnumerable<PhysicalAsset>>> GetPhysicalAssets()
        {
            var companyIdClaim = User.FindFirst("CompanyID")?.Value;
            if (!int.TryParse(companyIdClaim, out int companyId))
            {
                return Unauthorized(new { message = "Invalid CompanyID in token." });
            }

            var assets = await _physicalAssetRepository.GetAllAssetsAsync(companyId);
            return Ok(new { data = assets });
        }


        // POST: Create a new Physical Asset
        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<PhysicalAsset>> PostPhysicalAsset(PhysicalAsset asset)
        {
            await _physicalAssetRepository.AddAssetAsync(asset);
            return CreatedAtAction(nameof(GetPhysicalAssets), new { id = asset.AssetID }, new { message = "Asset created successfully.", data = asset });
        }

        // GET by id
        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<PhysicalAsset>> GetAssetById(int id)
        {
            var companyIdClaim = User.FindFirst("CompanyID")?.Value;
            if (!int.TryParse(companyIdClaim, out int companyId))
            {
                return Unauthorized(new { message = "Invalid CompanyID in token." });
            }

            var asset = await _physicalAssetRepository.GetAssetByIdAsync(id);
            if (asset == null || asset.CompanyID != companyId)
            {
                return NotFound(new { message = "Asset not found or access denied." });
            }

            return Ok(new { message = "Asset retrieved successfully.", data = asset });
        }


        // PUT: Update an existing Physical Asset
        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> PutPhysicalAsset(int id, PhysicalAsset asset)
        {
            if (id != asset.AssetID)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            // Set Status based on Quantity
            asset.Status = asset.Quantity == 0 ? "Unavailable" : "Available";

            await _physicalAssetRepository.UpdateAssetAsync(asset);
            return Ok(new { message = "Asset updated successfully." });
        }


        // DELETE: Delete a Physical Asset
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeletePhysicalAsset(int id)
        {
            var asset = await _physicalAssetRepository.GetAssetByIdAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Asset not found." });
            }

            await _physicalAssetRepository.DeleteAssetAsync(id);
            return Ok(new { message = "Asset deleted successfully." });
        }
    }
}