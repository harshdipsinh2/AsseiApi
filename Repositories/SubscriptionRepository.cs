using AssetManagement.Data;
using AssetManagement.Interfaces;
using AssetManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddSubscriptionAsync(EmployeeSubscription subscription)
        {
            await _context.tb_EmployeeSubscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();
        }

        // UPDATED: Get subscription by Email instead of EmployeeId
        public async Task<EmployeeSubscription> GetSubscriptionByEmailAsync(string email)
        {
            return await _context.tb_EmployeeSubscriptions.FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task UpdateSubscriptionStatusAsync(string subscriptionId, string status)
        {
            var subscription = await _context.tb_EmployeeSubscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);
            if (subscription != null)
            {
                subscription.Status = status;
                _context.tb_EmployeeSubscriptions.Update(subscription);
                await _context.SaveChangesAsync();
            }
        }
    }
}
