using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Repositories
{
    public class EmployeeSoftwareAssetRepository : IEmployeeSoftwareAssetRepository
    {
        private readonly AppDbContext _context;

        public EmployeeSoftwareAssetRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get all software asset assignments
        public async Task<IEnumerable<EmployeeSoftwareAsset>> GetAllAssignmentsAsync()
        {
            return await _context.tb_EmployeeSoftwareAssets
                .Select(e => new EmployeeSoftwareAsset
                {
                    EmployeeId = e.EmployeeId,
                    SoftwareID = e.SoftwareID,
                    EmployeeName = e.EmployeeName,
                    SoftwareName = e.SoftwareName,
                    AssignedDate = e.AssignedDate,
                    CompanyID = e.CompanyID

                })
                .ToListAsync();
        }

        // Get software asset assignments by employee ID
        public async Task<List<EmployeeSoftwareAsset>> GetEmployeeSoftwareAssetsByEmployeeIdAsync(int employeeId)
        {
            return await _context.tb_EmployeeSoftwareAssets
                .Where(e => e.EmployeeId == employeeId) // Filter by Employee ID
                .Join(_context.tb_Employees, e => e.EmployeeId, emp => emp.EmployeeId, (e, emp) => new { e, emp })
                .Join(_context.tb_SoftwareAssets, combined => combined.e.SoftwareID, s => s.SoftwareId, (combined, s) => new EmployeeSoftwareAsset
                {
                    EmployeeId = combined.e.EmployeeId,
                    SoftwareID = combined.e.SoftwareID,
                    EmployeeName = combined.emp.EmployeeName,
                    SoftwareName = s.SoftwareName,
                    AssignedDate = combined.e.AssignedDate, // Include AssignedDate
                    CompanyID = combined.e.CompanyID
                })
                .ToListAsync();
        }


        // Assign software asset to employee using stored procedure
        public async Task<string> AssignMultipleSoftwareAssetsToEmployeeAsync(int employeeId, List<int> softwareIds, DateTime assignedDate, int companyId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Assigning software assets: EmployeeId={employeeId}, SoftwareIDs={string.Join(",", softwareIds)}, AssignedDate={assignedDate}");

                var employee = await _context.tb_Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    Console.WriteLine("Employee not found.");
                    return "Employee not found.";
                }

                if (softwareIds == null || softwareIds.Count == 0)
                {
                    Console.WriteLine("No software assets provided.");
                    return "No software assets provided.";
                }

                string softwareIdString = string.Join(",", softwareIds);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC InsertEmployeeMultipleSoftwareAssets @EmployeeId={0}, @SoftwareIDs={1}, @AssignedDate={2}, @CompanyID={3}",
                    employeeId, softwareIdString, assignedDate, companyId
                );

                Console.WriteLine("[SUCCESS] Software assets assigned successfully.");
                return "Software assets assigned successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception: {ex.Message}");
                return "Failed to assign software assets.";
            }
        }





        // Delete software asset assignment for an employee
        public async Task<bool> DeleteEmployeeSoftwareAssetAsync(int employeeId, int softwareId)
        {
            // Find the assignment
            var employeeSoftwareAsset = await _context.tb_EmployeeSoftwareAssets
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SoftwareID == softwareId);

            if (employeeSoftwareAsset == null)
                return false;

            // Remove the assignment
            _context.tb_EmployeeSoftwareAssets.Remove(employeeSoftwareAsset);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
