using AssetManagement.Data;
using AssetManagement.Models;
using AssetManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Repositories
{
    public class SoftwareAssetRepository : ISoftwareAssetRepository
    {
        private readonly AppDbContext _context;

        public SoftwareAssetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SoftwareAsset>> GetAllSoftwareAssetsAsync()
        {
            return await _context.tb_SoftwareAssets.ToListAsync();
        }

        public async Task<SoftwareAsset> GetSoftwareAssetByIdAsync(int id)
        {
            return await _context.tb_SoftwareAssets.FindAsync(id);
        }

        public async Task AddSoftwareAssetAsync(SoftwareAsset softwareAsset)
        {
            await _context.tb_SoftwareAssets.AddAsync(softwareAsset);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSoftwareAssetAsync(SoftwareAsset softwareAsset)
        {
            var existingAsset = await _context.tb_SoftwareAssets
                                               .FirstOrDefaultAsync(s => s.SoftwareId == softwareAsset.SoftwareId);

            if (existingAsset != null)
            {
                // Detach the existing entity to prevent the "already being tracked" error
                _context.Entry(existingAsset).State = EntityState.Detached;

                // Now attach the updated entity to the context
                _context.Entry(softwareAsset).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteSoftwareAssetAsync(int id)
        {
            var softwareAsset = await _context.tb_SoftwareAssets.FindAsync(id);
            if (softwareAsset != null)
            {
                _context.tb_SoftwareAssets.Remove(softwareAsset);
                await _context.SaveChangesAsync();
            }
        }
    }
}
