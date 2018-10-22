using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Depanneur.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Depanneur.App.Hangfire
{
    public class ProcessSubscriptionsJob
    {
        private readonly DepanneurContext db;

        public ProcessSubscriptionsJob(DepanneurContext db)
        {
            this.db = db;
        }

        public Task RunDaily() => Run(SubscriptionFrequency.Daily);
        public Task RunWeekly() => Run(SubscriptionFrequency.Weekly);

        private async Task Run(SubscriptionFrequency frequency)
        {
            using (var tx = await db.Database.BeginTransactionAsync())
            {
                var subscriptions = await GetSubscriptions(frequency);

                foreach (var subscription in subscriptions)
                    Process(subscription);

                await db.SaveChangesAsync();
                tx.Commit();
            }
        }

        private void Process(Subscription subscription)
        {
            if (subscription.User.IsDeleted) return;
            if (subscription.Product.IsDeleted) return;

            var purchase = new Purchase {
                Timestamp = DateTime.UtcNow,
                User = subscription.User,
                Product = subscription.Product,
                ProductName = subscription.Product.Name,
                ProductPrice = subscription.Product.Price,
                Quantity = subscription.Quantity,
                Amount = subscription.Quantity * subscription.Product.Price,
                IsFromSubscription = true
            };

            purchase.NewBalance = subscription.User.Balance = subscription.User.Balance + purchase.Amount;

            db.Transactions.Add(purchase);
        }

        private Task<List<Subscription>> GetSubscriptions(SubscriptionFrequency frequency)
        {
            return db.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Product)
                .Where(s => s.Frequency == frequency)
                .ToListAsync();
        }
    }
}