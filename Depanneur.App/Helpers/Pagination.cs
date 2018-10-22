using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Depanneur.App.Helpers
{
    public static class PaginationExtensions
    {
        public static Task<Pagination<T>> GetPageAsync<T>(this IOrderedQueryable<T> query, int pageNumber, int pageSize)
        {
            return query.GetPageAsync(pageNumber, pageSize, x => x, x => x);
        }

        public static Task<Pagination<TResult>> GetPageAsync<TSource, TResult>(this IOrderedQueryable<TSource> query, int pageNumber, int pageSize, Expression<Func<TSource, TResult>> serverProjection)
        {
            return query.GetPageAsync(pageNumber, pageSize, serverProjection, x => x);
        }

        public static async Task<Pagination<TResult>> GetPageAsync<TSource, TProjection, TResult>(this IOrderedQueryable<TSource> query, int pageNumber, int pageSize, Expression<Func<TSource, TProjection>> serverProjection, Func<TProjection, TResult> clientProjection)
        {
            var totalCount = await query.CountAsync();

            var items = await query
                .Select(serverProjection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Pagination<TResult>(items.Select(clientProjection), totalCount, pageSize, pageNumber);
        }
    }
    
    public class Pagination<T>
    {
        public Pagination(IEnumerable<T> items, int total, int size, int current)
        {
            Items = items.ToList();
            TotalItems = total;
            PageSize = size;
            CurrentPage = current;
        }

        public IList<T> Items { get; }
        public int TotalItems { get; }
        public int PageSize { get; }
        public int CurrentPage { get; }
        public int PageCount => (int) Math.Ceiling((float) TotalItems / PageSize);
    }
}
