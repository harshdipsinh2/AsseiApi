using Microsoft.EntityFrameworkCore;
using AssetManagement.Data;
using AssetManagement.Models;
using AssetManagement.Interfaces;

namespace AssetManagement.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesAsync(int companyId)
        {
            return await _context.tb_Employees
                .Where(e => e.CompanyID == companyId)
                .ToListAsync();
        }


        public async Task<Employee> GetEmployeeByIdAsync(int id, int companyId)
        {
            return await _context.tb_Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id && e.CompanyID == companyId);
        }


        public async Task AddEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            await _context.tb_Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateEmployeeAsync(Employee employee)
        {
            var existingEmployee = await _context.tb_Employees.FindAsync(employee.EmployeeId);
            if (existingEmployee == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }

            // Update all required employee fields
            existingEmployee.EmployeeName = employee.EmployeeName;
            existingEmployee.Dept = employee.Dept;
            existingEmployee.JoinDate = employee.JoinDate;
            existingEmployee.Salary = employee.Salary;
            existingEmployee.RoleID = employee.RoleID;
            existingEmployee.PhoneNumber = employee.PhoneNumber;
            existingEmployee.EmailId = employee.EmailId;

            // Update related user info, if linked
            var user = await _context.tb_Users.FirstOrDefaultAsync(u => u.EmployeeID == employee.EmployeeId);
            if (user != null)
            {
                user.PhoneNumber = employee.PhoneNumber;
                user.Email = employee.EmailId;
            }

            await _context.SaveChangesAsync();
        }



        // Method to delete Asset Requests by Employee ID
        public async Task DeleteAssetRequestsByEmployeeIdAsync(int employeeId)
        {
            // Find all asset requests linked to this employee
            var assetRequests = await _context.tb_AssetRequests
                .Where(ar => ar.EmployeeID == employeeId)
                .ToListAsync();

            if (assetRequests.Any())
            {
                // Remove them from the table
                _context.tb_AssetRequests.RemoveRange(assetRequests);
                await _context.SaveChangesAsync();
            }
        }

        // Method to delete the employee and related references
        public async Task DeleteEmployeeAsync(int id)
        {
            // Delete the asset requests linked to the employee
            await DeleteAssetRequestsByEmployeeIdAsync(id);

            // Now delete the employee
            var employee = await _context.tb_Employees.FindAsync(id);
            if (employee != null)
            {
                _context.tb_Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<bool> HasAssignedAssetsAsync(int employeeId)
        {
            return await _context.tb_EmployeePhysicalAssets.AnyAsync(a => a.EmployeeId == employeeId) ||
                   await _context.tb_EmployeeSoftwareAssets.AnyAsync(a => a.EmployeeId == employeeId);
        }


        public async Task<bool> EmployeeExistsAsync(int id, int companyId)
        {
            return await _context.tb_Employees.AnyAsync(e => e.EmployeeId == id && e.CompanyID == companyId);
        }
        }
    }
