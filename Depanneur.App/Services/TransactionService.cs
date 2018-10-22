
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Depanneur.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Depanneur.App.Services
{
    public class TransactionService
    {
        public static TimeSpan DeletePeriod = TimeSpan.FromHours(12);

        private readonly DepanneurContext db;

        public TransactionService(DepanneurContext db)
        {
            this.db = db;
        }

        public async Task<Purchase> PurchaseProduct(string userId, int productId, int quantity)
        {
            using (var tx = await db.Database.BeginTransactionAsync())
            {
                if (quantity < 1) throw new ArgumentException("Quantity must be greater or equal to 1", nameof(quantity)); ;

                var product = await db.Products.FindAsync(productId);
                if (product == null) throw new ArgumentException($"Unknown product: {productId}", nameof(productId));
                if (product.IsDeleted) throw new InvalidOperationException($"Product {productId} is deleted");

                var user = await db.Users.FindAsync(userId);
                if (user == null) throw new ArgumentException($"Unknown user: {userId}", nameof(userId));
                if (user.IsDeleted) throw new InvalidOperationException($"User {userId} is deleted");

                var purchase = new Purchase
                {
                    Product = product,
                    Quantity = quantity,
                    ProductPrice = product.Price,
                    ProductName = product.Name,
                    Amount = quantity * product.Price
                };

                RecordTransaction(purchase, user);

                await db.SaveChangesAsync();
                tx.Commit();

                return purchase;
            }
        }

        public async Task<Adjustment> AddAdjustment(string userId, decimal amount, string description)
        {
            using (var tx = await db.Database.BeginTransactionAsync()) 
            {
                var user = await db.Users.FindAsync(userId);
                if (user == null) throw new ArgumentException($"Unknown user: {userId}", nameof(userId));

                var adjustment = new Adjustment
                {
                    Amount = amount,
                    Description = description
                };

                RecordTransaction(adjustment, user);

                await db.SaveChangesAsync();
                tx.Commit();

                return adjustment;
            }
        }

        public async Task<Payment> AddPayment(string userId, decimal amount) 
        {
            using (var tx = await db.Database.BeginTransactionAsync()) 
            {
                var user = await db.Users.FindAsync(userId);
                if (user == null) throw new ArgumentException($"Unknown user: {userId}", nameof(userId));

                var payment = new Payment
                {
                    Amount = -amount
                };

                RecordTransaction(payment, user);

                await db.SaveChangesAsync();
                tx.Commit();

                return payment;
            }
        }

        public bool CanCancel(string userId, Transaction tx) 
        {
            return tx is Purchase && 
                   tx.UserId == userId && 
                   tx.Timestamp > DateTime.UtcNow - DeletePeriod;
        }

        public async Task<IEnumerable<Purchase>> CancelPurchases(string userId, int[] purchaseIds)
        {
            using (var tx = await db.Database.BeginTransactionAsync())
            {
                var user = await db.Users.FindAsync(userId);
                if (user == null) throw new ArgumentException($"Unknown user: {userId}", nameof(userId));
                if (user.IsDeleted) throw new InvalidOperationException($"User {userId} is deleted");

                var purchases = await db.Transactions.OfType<Purchase>()
                    .Where(p => p.UserId == userId)
                    .Where(p => purchaseIds.Contains(p.Id))
                    .ToListAsync();

                if (purchases.Count != purchaseIds.Length) throw new InvalidOperationException("Purchases not found");
                if (purchases.Any(p => p.Timestamp < DateTime.UtcNow - DeletePeriod)) throw new InvalidOperationException("Cancellation period has expired");

                user.Balance = user.Balance - purchases.Sum(p => p.Amount);
                db.Transactions.RemoveRange(purchases);
                
                await db.SaveChangesAsync();
                tx.Commit();

                return purchases;
            }
        }

        private void RecordTransaction(Transaction t, User u) 
        {
            t.User = u;
            t.NewBalance = u.Balance = u.Balance + t.Amount;
            t.Timestamp = DateTime.UtcNow;

            db.Transactions.Add(t);
        }
    }
}