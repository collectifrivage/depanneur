using System.Linq;
using Depanneur.App.Entities;

namespace Depanneur.App.Data
{
    public class TransactionRepository : Repository<Transaction>
    {
        public TransactionRepository(DepanneurContext db) : base(db)
        {
        }
    }

    public static class TransactionExtensions
    {
        public static IQueryable<T> ForUser<T>(this IQueryable<T> source, string userId) where T : Transaction
        {
            return source.Where(x => x.UserId == userId);
        }

        public static IQueryable<Purchase> PurchasesOfProduct(this IQueryable<Transaction> source, int productId)
        {
            return source.OfType<Purchase>().Where(p => p.ProductId == productId);
        }

        public static IOrderedQueryable<T> Chronologically<T>(this IQueryable<T> source) where T : Transaction
        {
            return source.OrderByDescending(x => x.Timestamp);
        }

        public static IQueryable<Transaction> IncludeTypes(this IQueryable<Transaction> source, bool purchases, bool payments, bool adjustments)
        {
            return source
                .Where(x => purchases || !(x is Purchase))
                .Where(x => payments || !(x is Payment))
                .Where(x => adjustments || !(x is Adjustment));
        }
    }
}