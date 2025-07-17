using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface IAssetRequestRepository
    {
        Task<IEnumerable<AssetRequest>> GetPendingRequestsAsync();
        Task<IEnumerable<AssetRequest>> GetRequestHistoryAsync();
        Task RequestAssetAsync(int employeeId, string assetName, int companyId);
        Task ApproveAssetRequestAsync(int requestId, int assetId);
        Task RejectAssetRequestAsync(int requestId, int assetId);
    }
}
