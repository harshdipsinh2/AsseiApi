using Microsoft.EntityFrameworkCore;
using AssetManagement.Data;
using AssetManagement.Models;
using AssetManagement.Interfaces;

namespace AssetManagement.Repositories
{
    public class PhysicalAssetRepository : IPhysicalAssetRepository
    {
        private readonly AppDbContext _context;

        public PhysicalAssetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhysicalAsset>> GetAllAssetsAsync(int companyId)
        {
            return await _context.tb_Assets
                .Where(asset => asset.CompanyID == companyId)
                .ToListAsync();
        }


        public async Task<PhysicalAsset> GetAssetByIdAsync(int id)
        {
            return await _context.tb_Assets.FindAsync(id);
        }

        public async Task AddAssetAsync(PhysicalAsset asset)
        {
            await _context.tb_Assets.AddAsync(asset);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAssetAsync(PhysicalAsset asset)
        {
            _context.Entry(asset).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAssetAsync(int id)
        {
            var asset = await _context.tb_Assets.FindAsync(id);
            if (asset != null)
            {
                _context.tb_Assets.Remove(asset);
                await _context.SaveChangesAsync();
            }
        }
    }
}