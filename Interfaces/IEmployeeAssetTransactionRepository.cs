using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface IEmployeeAssetTransactionRepository
    {
        Task<IEnumerable<EmployeeAssetTransaction>> GetAllTransactionsAsync();
        Task AddTransactionAsync(EmployeeAssetTransaction transaction);
        Task<IEnumerable<EmployeeAssetTransaction>> GetTransactionsByEmployeeIdAsync(int employeeId);
        Task<Employee> GetEmployeeByIdAsync(int employeeId);
        Task<bool> HasEmployeeBoughtAsset(int employeeId, int assetId);


    }
}
