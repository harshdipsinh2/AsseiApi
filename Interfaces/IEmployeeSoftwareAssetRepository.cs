using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface IEmployeeSoftwareAssetRepository
    {
        Task<IEnumerable<EmployeeSoftwareAsset>> GetAllAssignmentsAsync();
        Task<string> AssignMultipleSoftwareAssetsToEmployeeAsync(int employeeId, List<int> softwareIds, DateTime assignedDate, int companyId);
        Task<List<EmployeeSoftwareAsset>> GetEmployeeSoftwareAssetsByEmployeeIdAsync(int employeeId);
        Task<bool> DeleteEmployeeSoftwareAssetAsync(int employeeId, int softwareId);
    }
}
