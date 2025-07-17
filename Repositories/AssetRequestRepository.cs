using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Repositories
{
    public class AssetRequestRepository : IAssetRequestRepository
    {
        private readonly AppDbContext _context;

        public AssetRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssetRequest>> GetPendingRequestsAsync()
        {
            return await _context.tb_AssetRequests
                .Where(a => a.Status == "Pending")
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetRequest>> GetRequestHistoryAsync()
        {
            return await _context.tb_AssetRequests
                .Where(a => a.Status == "Approved" || a.Status == "Rejected")
                .OrderByDescending(a => a.RequestedDate)
                .ToListAsync();
        }

        public async Task RequestAssetAsync(int employeeId, string assetName, int companyId)
        {
            var request = new AssetRequest
            {
                EmployeeID = employeeId,
                AssetName = assetName,
                CompanyID = companyId
            };

            await _context.tb_AssetRequests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task ApproveAssetRequestAsync(int requestId, int assetId)
        {
            var request = await _context.tb_AssetRequests.FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null)
            {
                throw new Exception($"Asset request with ID {requestId} not found.");
            }

            request.Status = "Approved";
            request.AssetID = assetId;
            request.ApprovalDate = DateTime.Now;  // Set approval date

            await _context.SaveChangesAsync();
        }



        public async Task RejectAssetRequestAsync(int requestId, int assetId)
        {
            var request = await _context.tb_AssetRequests.FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null)
            {
                throw new Exception($"Asset request with ID {requestId} not found.");
            }

            // If needed, fetch asset details (only if rejection logic involves asset-specific actions)
            var asset = await _context.tb_Assets.FirstOrDefaultAsync(a => a.AssetID == assetId);
            if (asset == null)
            {
                throw new Exception($"Asset with ID {assetId} not found.");
            }

            request.Status = "Rejected";
            request.AssetID = assetId;
            request.ApprovalDate = DateTime.Now; //  Store rejection date

            await _context.SaveChangesAsync();
        }
    }
}

