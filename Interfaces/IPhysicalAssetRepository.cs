using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface IPhysicalAssetRepository
    {
        Task<IEnumerable<PhysicalAsset>> GetAllAssetsAsync(int companyId);
        Task<PhysicalAsset> GetAssetByIdAsync(int id);
        Task AddAssetAsync(PhysicalAsset asset);
        Task UpdateAssetAsync(PhysicalAsset asset);
        Task DeleteAssetAsync(int id);
    }
}
