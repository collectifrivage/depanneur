using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Depanneur.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Depanneur.App.Data
{
    public class SubscriptionRepository : Repository<Subscription>
    {
        public SubscriptionRepository(DepanneurContext db) : base(db) { }

        public IQueryable<Subscription> UserSubscriptions(string userId) => GetAll().Where(x => x.UserId == userId);


        public async Task<IDictionary<int, bool>> AreProductsSubscribed(string userId, IEnumerable<int> productIds)
        {
            var subscribed = await UserSubscriptions(userId)
                .Select(x => x.ProductId)
                .ToListAsync()
                .ConfigureAwait(false);

            return productIds.ToDictionary(x => x, x => subscribed.Contains(x));
        }

        public async Task<Subscription> GetProductSubscription(string userId, int productId)
        {
            var result = await GetProductSubscriptions(userId, new[] { productId });
            return result[productId];
        }

        public async Task<IDictionary<int, Subscription>> GetProductSubscriptions(string userId, IEnumerable<int> productIds)
        {
            var subscriptions = await GetAll()
                .Where(s => s.UserId == userId)
                .Where(s => productIds.Contains(s.ProductId))
                .ToDictionaryAsync(x => x.ProductId)
                .ConfigureAwait(false);

            return productIds.ToDictionary(x => x, x => subscriptions.GetValueOrDefault(x));
        }

        public Subscription CreateOrUpdate(string userId, int productId, int quantity, SubscriptionFrequency frequency)
        {
            var sub = GetAll().FirstOrDefault(x => x.UserId == userId && x.ProductId == productId);

            if (sub == null)
            {
                sub = new Subscription
                {
                    UserId = userId,
                    ProductId = productId
                };

                db.Subscriptions.Add(sub);
            }

            sub.Quantity = quantity;
            sub.Frequency = frequency;

            db.SaveChanges();

            return sub;
        }
    }
}