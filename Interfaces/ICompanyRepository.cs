using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company> AddCompanyAsync(Company company);
        Task<Company?> GetCompanyByIdAsync(int companyId);
        Task<Company?> GetCompanyByNameAsync(string companyName);
        Task<List<Company>> GetAllCompaniesAsync();

    }
}
