using System.Collections.Generic;
using Depanneur.App.Entities;
using GraphQL.Types;

namespace Depanneur.App.Schema.Payloads
{
    public class BatchCancellationResultType : ObjectGraphType<BatchCancellationResultType.Data>
    {
        public class Data
        {
            public User User { get; set; }
            public ICollection<Product> Products { get; set; }
        }

        public BatchCancellationResultType()
        {
            Name = "BatchCancellationResult";
            Description = "Result of cancelling a list of purchases.";

            Field<UserType, User>("user").Resolve(ctx => ctx.Source.User).Description("The user that was associated with the cancelled purchases.");
            Field<ListGraphType<ProductType>, IEnumerable<Product>>("products").Resolve(ctx => ctx.Source.Products).Description("The products that were purchased in the cancelled purchases.");
        }
    }
}