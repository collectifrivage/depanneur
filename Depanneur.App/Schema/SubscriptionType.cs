using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Helpers;
using GraphQL.Types;

namespace Depanneur.App.Schema
{
    public class SubscriptionType : ObjectGraphType<Subscription> 
    {
        public SubscriptionType(DataLoader loader, ProductRepository products, UserRepository users)
        {
            Name = "Subscription";
            Description = "Details about a subscription.";

            Field(x => x.Id).Description("The ID of the subscription.");
            Field<NonNullGraphType<EnumerationGraphType<SubscriptionFrequency>>>("frequency", resolve: ctx => ctx.Source.Frequency, description: "The subscription's frequency.");
            Field(x => x.Quantity).Description("The quantity that is purchased at the given frequency.");
            Field<NonNullGraphType<ProductType>>("product", resolve: ctx => loader.LoadBatch("GetProductsById", ctx.Source.ProductId, products.GetProductsById), description: "The product that is subscribed to.");
            Field<NonNullGraphType<UserType>>("user", resolve: ctx => loader.LoadBatch("GetUsersById", ctx.Source.UserId, users.GetUsersById), description: "The user who owns this subscription.");
        }
    }
}