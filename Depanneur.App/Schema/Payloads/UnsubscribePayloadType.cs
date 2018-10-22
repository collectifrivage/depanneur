using Depanneur.App.Data;
using Depanneur.App.Helpers;
using GraphQL.Types;

namespace Depanneur.App.Schema.Payloads
{
    public class UnsubscribePayloadType : ObjectGraphType<UnsubscribePayloadType.Data>
    {
        public class Data
        {
            public string UserId { get; set; }
            public int ProductId { get; set; }
        }

        public UnsubscribePayloadType(DataLoader loader, UserRepository users, ProductRepository products)
        {
            Name = "UnsubscribePayload";
            Description = "Result of unsubscribing from a product.";

            Field<UserType>("user", resolve: ctx => loader.LoadBatch("GetUsersById", ctx.Source.UserId, users.GetUsersById), description: "The user who unsubscribed.");
            Field<ProductType>("product", resolve: ctx => loader.LoadBatch("GetProductsById", ctx.Source.ProductId, products.GetProductsById), description: "The product that was unsubscribed from.");
        }
    }
}