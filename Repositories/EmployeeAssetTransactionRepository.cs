using Microsoft.EntityFrameworkCore;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using AssetManagement.Data;

namespace AssetManagement.Repositories
{
    public class EmployeeAssetTransactionRepository : IEmployeeAssetTransactionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmployeeAssetTransactionRepository> _logger; // Add logger

        public EmployeeAssetTransactionRepository(AppDbContext context, ILogger<EmployeeAssetTransactionRepository> logger)
        {
            _context = context;
            _logger = logger; // Inject logger
        }

        public async Task<IEnumerable<EmployeeAssetTransaction>> GetAllTransactionsAsync()
        {
            return await _context.tb_EmployeeAssetTransactions.ToListAsync();
        }


        public async Task AddTransactionAsync(EmployeeAssetTransaction transaction)
        {
            _logger.LogInformation("AddTransactionAsync called."); // Log information

            try
            {
                _context.tb_EmployeeAssetTransactions.Add(transaction);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Transaction saved successfully."); // Log success
                }
                else
                {
                    _logger.LogWarning("No changes were made."); // Log warning
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transaction."); // Log error
                throw; // Re-throw the exception to be handled by the controller/service
            }
        }


        public async Task<IEnumerable<EmployeeAssetTransaction>> GetTransactionsByEmployeeIdAsync(int employeeId)
        {
            return await _context.tb_EmployeeAssetTransactions
                .Where(t => t.EmployeeId == employeeId)
                .ToListAsync();
        }
            public async Task<Employee> GetEmployeeByIdAsync(int employeeId)
        {
            return await _context.tb_Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> HasEmployeeBoughtAsset(int employeeId, int assetId)
        {
            return await _context.tb_EmployeeAssetTransactions
                .AnyAsync(t => t.EmployeeId == employeeId && t.AssetID == assetId);
        }

    }
}
