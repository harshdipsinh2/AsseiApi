using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Repository
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly AppDbContext _context;

        public CompanyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Company> AddCompanyAsync(Company company)
        {
            _context.tb_Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<Company?> GetCompanyByIdAsync(int companyId)
        {
            return await _context.tb_Companies.FindAsync(companyId);
        }

        public async Task<Company?> GetCompanyByNameAsync(string companyName)
        {
            return await _context.tb_Companies.FirstOrDefaultAsync(c => c.CompanyName == companyName);
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            return await _context.tb_Companies.ToListAsync();
        }

    }
}
