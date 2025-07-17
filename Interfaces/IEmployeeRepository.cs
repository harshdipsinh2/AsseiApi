using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetEmployeesAsync(int companyId);
        Task<Employee> GetEmployeeByIdAsync(int id, int companyId);
        Task<bool> EmployeeExistsAsync(int id, int companyId);
        Task AddEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(int id);
        Task<bool> HasAssignedAssetsAsync(int employeeId);

    }
}
