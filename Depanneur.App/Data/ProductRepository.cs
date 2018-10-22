using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Depanneur.App.Entities;
using Depanneur.App.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Depanneur.App.Data
{
    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(DepanneurContext db) : base(db)
        {
        }

        public IQueryable<PurchaseStatistics> GetPurchaseStatistics(string userId)
        {
            var monthStart = DateTime.UtcNow.UtcToLocal().StartOfMonth();

            return db.Products
                .Select(p => new PurchaseStatistics
                {
                    ProductId = p.Id,
                    Month = p.Purchases.Where(tx => tx.User.Id == userId && tx.Timestamp >= monthStart).Sum(tx => tx.Quantity),
                    Total = p.Purchases.Where(tx => tx.User.Id == userId).Sum(tx => tx.Quantity)
                });
        }

        public async Task<IDictionary<int, PurchaseStatistics>> GetPurchaseStatistics(string userId, IEnumerable<int> productIds)
        {
            return await GetPurchaseStatistics(userId)
                .Where(x => productIds.Contains(x.ProductId))
                .ToDictionaryAsync(x => x.ProductId)
                .ConfigureAwait(false);
        }

        public async Task<IDictionary<int, int?>> GetWeeklyAverages(string userId, IEnumerable<int> productIds)
        {
            var ids = productIds.ToList();

            var end = GetWeekStart(DateTime.UtcNow.Date);
            var start = end.AddDays(-45);

            // Obtenir la consommation quotidienne pour les 45 derniers jours. Note: ceci ne considère pas les timezones, mais ça ne devrait pas affecter les résultats
            var quantitiesByDate = await db.Products
                .Where(p => ids.Contains(p.Id))
                .SelectMany(p => p.Purchases)
                .Where(p => p.UserId == userId)
                .Where(p => p.Timestamp > start && p.Timestamp < end)
                .Select(p => new { p.ProductId, p.Timestamp.Date, p.Quantity })
                .GroupBy(p => new { p.ProductId, p.Date })
                .Select(g => new { g.Key.ProductId, g.Key.Date, Quantity = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // Grouper les résultats par semaine
            var quantitiesByWeek = quantitiesByDate
                .GroupBy(x => new { x.ProductId, Week = GetWeekStart(x.Date) })
                .Select(g => new { g.Key.ProductId, g.Key.Week, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            // Pour les produits avec au moins 3 semaines d'historique, faire la somme des 3 dernières semaines
            var totalsLast3Weeks = quantitiesByWeek
                .GroupBy(x => x.ProductId)
                .Where(x => x.Count() >= 3)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(g => g.Week).Take(3).Sum(g => g.Quantity));

            return ids.ToDictionary(
                id => id,
                id => totalsLast3Weeks.ContainsKey(id)
                    ? (int)Math.Ceiling(totalsLast3Weeks[id] / 3f)
                    : (int?)null);
        }

        public async Task<IDictionary<int, Product>> GetProductsById(IEnumerable<int> productIds)
        {
            return await GetAll()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id)
                .ConfigureAwait(false);
        }

        private DateTime GetWeekStart(DateTime date) => date.AddDays((int)date.DayOfWeek * -1);
    }

    public class PurchaseStatistics
    {
        public int ProductId { get; set; }
        public int Month { get; set; }
        public int Total { get; set; }
    }

    public static class ProductExtensions
    {
        public static IOrderedQueryable<Product> SortedByTotalUsage(this IQueryable<Product> products, string userId)
        {
            Expression<Func<Product, int>> totalPurchases = p => p.Purchases.Where(t => t.User.Id == userId && t.ProductId == p.Id).Sum(t => t.Quantity);

            return products.OrderByDescending(totalPurchases);
        }
    }
}