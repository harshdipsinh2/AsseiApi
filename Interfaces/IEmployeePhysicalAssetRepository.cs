using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface IEmployeePhysicalAssetRepository
    {
        Task<List<EmployeePhysicalAsset>> GetEmployeePhysicalAssetsAsync();
        Task<List<EmployeePhysicalAsset>> GetEmployeePhysicalAssetsByEmployeeIdAsync(int employeeId);
        Task<string> AssignMultiplePhysicalAssetsToEmployeeAsync(int employeeId, List<int> assetIds, DateTime assignedDate, int companyId);

        Task<string> TransferPhysicalAssetAsync(int oldEmployeeId, int newEmployeeId, int assetId);
        Task<bool> DeleteEmployeePhysicalAssetAsync(int employeeId, int assetId);
    }
}