using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface ISoftwareAssetRepository
    {
        Task<IEnumerable<SoftwareAsset>> GetAllSoftwareAssetsAsync();
        Task<SoftwareAsset> GetSoftwareAssetByIdAsync(int id);
        Task AddSoftwareAssetAsync(SoftwareAsset softwareAsset);
        Task UpdateSoftwareAssetAsync(SoftwareAsset softwareAsset);
        Task DeleteSoftwareAssetAsync(int id);
    }
}
