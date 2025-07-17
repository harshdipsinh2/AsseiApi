using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AssetManagement.Interfaces;
using AssetManagement.Models;

namespace AssetManagement.Controllers
{
    [Authorize] // Require authentication for all endpoints
    [Route("api/[controller]")]
    [ApiController]
    public class AssetRequestController : ControllerBase
    {
        private readonly IAssetRequestRepository _repository;

        public AssetRequestController(IAssetRequestRepository repository)
        {
            _repository = repository;
        }

        // GET: api/AssetRequest/pending
        [HttpGet("pending")]
        [Authorize(Roles = "1,2")] // Super Admin & Admin can view pending requests
        public async Task<IActionResult> GetPendingRequests()
        {
            var requests = await _repository.GetPendingRequestsAsync();
            return Ok(requests);
        }

        // GET: api/AssetRequest/history
        [HttpGet("history")]
        [Authorize(Roles = "1,2")] // Super Admin & Admin can view request history
        public async Task<IActionResult> GetRequestHistory()
        {
            var history = await _repository.GetRequestHistoryAsync();
            return Ok(history);
        }

        // POST: api/AssetRequest/request
        [HttpPost("request")]
        [Authorize(Roles = "2,3")] // Only Admin and Employee can request an asset
        public async Task<IActionResult> RequestAsset([FromBody] AssetRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request data.");

            // 🔹 Extract EmployeeID and CompanyID from the token
            var employeeIdClaim = User.FindFirst("EmployeeID")?.Value;
            var companyIdClaim = User.FindFirst("CompanyID")?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim) || string.IsNullOrEmpty(companyIdClaim))
                return Unauthorized("Employee ID or Company ID not found in token.");

            request.EmployeeID = int.Parse(employeeIdClaim);
            request.CompanyID = int.Parse(companyIdClaim);

            await _repository.RequestAssetAsync(request.EmployeeID, request.AssetName, request.CompanyID);
            return Ok("Asset request submitted successfully.");
        }



        // POST: api/AssetRequest/approve/{requestId}
        [HttpPost("approve/{requestId}/{assetId}")]
        [Authorize(Roles = "1")] // Only Super Admin can approve requests
        public async Task<IActionResult> ApproveAssetRequest(int requestId, int assetId)
        {
            try
            {
                await _repository.ApproveAssetRequestAsync(requestId, assetId);
                return Ok("Asset request approved.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/AssetRequest/reject/{requestId}
        // Corrected Reject Route
        [HttpPost("reject/{requestId}/{assetId}")]
        [Authorize(Roles = "1")] // Only Super Admin can reject requests
        public async Task<IActionResult> RejectAssetRequest(int requestId, int assetId)
        {
            try
            {
                await _repository.RejectAssetRequestAsync(requestId, assetId);
                return Ok("Asset request rejected.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

