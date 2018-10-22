using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.DataLoader;

namespace Depanneur.App.Helpers
{
    public class DataLoader
    {
        private readonly IDataLoaderContextAccessor accessor;

        public DataLoader(IDataLoaderContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public Task<TResult> LoadBatch<TId, TResult>(string uniqueKey, TId id, Func<IEnumerable<TId>, Task<IDictionary<TId, TResult>>> getBatch)
        {
            var loader = accessor.Context.GetOrAddBatchLoader(uniqueKey, getBatch);
            return loader.LoadAsync(id);
        }
    }
}