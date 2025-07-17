using AssetManagement.Models;

namespace AssetManagement.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task AddSubscriptionAsync(EmployeeSubscription subscription);

        // Updated: Get subscription by Email instead of EmployeeId
        Task<EmployeeSubscription> GetSubscriptionByEmailAsync(string email);

        Task UpdateSubscriptionStatusAsync(string subscriptionId, string status);
    }
}