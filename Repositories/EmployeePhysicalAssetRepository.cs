using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Repositories
{
    public class EmployeePhysicalAssetRepository : IEmployeePhysicalAssetRepository
    {
        private readonly AppDbContext _context;

        public EmployeePhysicalAssetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> AssignMultiplePhysicalAssetsToEmployeeAsync(int employeeId, List<int> assetIds, DateTime assignedDate, int companyId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Assigning assets: EmployeeId={employeeId}, Assets={string.Join(",", assetIds)}, AssignedDate={assignedDate}");

                var employee = await _context.tb_Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    Console.WriteLine(" Employee not found.");
                    return "Employee not found.";
                }

                if (assetIds == null || assetIds.Count == 0)
                {
                    Console.WriteLine(" No assets provided.");
                    return "No assets provided.";
                }

                string assetIdString = string.Join(",", assetIds);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC InsertEmployeeMultiplePhysicalAssets @EmployeeId={0}, @AssetIDs={1}, @AssignedDate={2}, @CompanyID={3}",
                    employeeId, assetIdString, assignedDate, companyId);


                // 🔹 **Manually Check If Assets Were Actually Assigned**
                var assignedAssets = await _context.tb_EmployeePhysicalAssets
                    .Where(e => e.EmployeeId == employeeId && assetIds.Contains(e.AssetId))
                    .ToListAsync();

                if (assignedAssets.Count > 0) // If assets exist in the table after the procedure call
                {
                    //  Update Status for assets with Quantity == 0
                    var assets = await _context.tb_Assets
                        .Where(a => assetIds.Contains(a.AssetID))
                        .ToListAsync();

                    foreach (var asset in assets)
                    {
                        if (asset.Quantity == 0)
                        {
                            asset.Status = "Unavailable"; // Change status when Quantity is 0
                        }
                    }

                    await _context.SaveChangesAsync(); //  Save changes to DB

                    Console.WriteLine("[SUCCESS] Assets assigned successfully.");
                    return "Assets assigned successfully.";
                }
                else
                {
                    Console.WriteLine("[ERROR] No assets assigned.");
                    return "No assets were assigned.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception: {ex.Message}");
                return "Failed to assign assets.";
            }
        }


        public async Task<string> TransferPhysicalAssetAsync(int oldEmployeeId, int newEmployeeId, int assetId)
        {
            try
            {
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "EXEC TransferEmployeePhysicalAsset @OldEmployeeId, @NewEmployeeId, @AssetId";
                        command.CommandType = System.Data.CommandType.Text;

                        // Add parameters
                        var paramOldEmployeeId = command.CreateParameter();
                        paramOldEmployeeId.ParameterName = "@OldEmployeeId";
                        paramOldEmployeeId.Value = oldEmployeeId;
                        command.Parameters.Add(paramOldEmployeeId);

                        var paramNewEmployeeId = command.CreateParameter();
                        paramNewEmployeeId.ParameterName = "@NewEmployeeId";
                        paramNewEmployeeId.Value = newEmployeeId;
                        command.Parameters.Add(paramNewEmployeeId);

                        var paramAssetId = command.CreateParameter();
                        paramAssetId.ParameterName = "@AssetId";
                        paramAssetId.Value = assetId;
                        command.Parameters.Add(paramAssetId);

                        // Execute and get the string message
                        var result = await command.ExecuteScalarAsync();

                        return result?.ToString() ?? "Unknown error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }


        public async Task<bool> DeleteEmployeePhysicalAssetAsync(int employeeId, int assetId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Fetch the specific record's Id to delete
                var idToDelete = await _context.tb_EmployeePhysicalAssets
                    .Where(e => e.EmployeeId == employeeId && e.AssetId == assetId)
                    .OrderBy(e => e.Id)  // Ensures deterministic deletion
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync();

                if (idToDelete == 0) // If no record found, return false
                {
                    Console.WriteLine("No matching record found to delete.");
                    return false;
                }

                Console.WriteLine($"Deleting record with Id: {idToDelete}");

                // DELETE only one instance
                int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tb_EmployeePhysicalAssets WHERE Id = {0}",
                    idToDelete); // Pass as an indexed parameter

                if (rowsAffected == 0)
                {
                    Console.WriteLine("Delete failed: No rows affected.");
                    return false;
                }

                Console.WriteLine("Record deleted successfully.");

                // Update the asset quantity safely
                int updateResult = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE tb_Assets SET Quantity = Quantity + 1 WHERE AssetID = {0}",
                    assetId); // Pass assetId correctly

                if (updateResult == 0)
                {
                    Console.WriteLine("Quantity update failed.");
                    return false;
                }

                Console.WriteLine("Asset quantity updated successfully.");

                // Commit transaction if all operations succeed
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<EmployeePhysicalAsset>> GetEmployeePhysicalAssetsAsync()
        {
            return await _context.tb_EmployeePhysicalAssets
                .Join(_context.tb_Employees, e => e.EmployeeId, emp => emp.EmployeeId, (e, emp) => new { e, emp })
                .Join(_context.tb_Assets, combined => combined.e.AssetId, a => a.AssetID, (combined, a) => new EmployeePhysicalAsset
                {
                    EmployeeId = combined.e.EmployeeId,
                    AssetId = combined.e.AssetId,
                    EmployeeName = combined.emp.EmployeeName,
                    AssetName = a.AssetName,
                    AssignedDate = combined.e.AssignedDate,
                    CompanyID = combined.e.CompanyID
                })
                .ToListAsync();
        }

        public async Task<List<EmployeePhysicalAsset>> GetEmployeePhysicalAssetsByEmployeeIdAsync(int employeeId)
        {
            return await _context.tb_EmployeePhysicalAssets
                .Where(e => e.EmployeeId == employeeId) // Filter by Employee ID
                .Join(_context.tb_Employees, e => e.EmployeeId, emp => emp.EmployeeId, (e, emp) => new { e, emp })
                .Join(_context.tb_Assets, combined => combined.e.AssetId, a => a.AssetID, (combined, a) => new EmployeePhysicalAsset
                {
                    EmployeeId = combined.e.EmployeeId,
                    AssetId = combined.e.AssetId,
                    EmployeeName = combined.emp.EmployeeName,
                    AssetName = a.AssetName,
                    AssignedDate = combined.e.AssignedDate, // Include AssignedDate
                    CompanyID = combined.e.CompanyID
                })
                .ToListAsync();
        }
    }
}
