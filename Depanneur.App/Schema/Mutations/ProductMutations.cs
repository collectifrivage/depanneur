using System.Security.Claims;
using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Models;
using Depanneur.App.Schema.Inputs;
using Depanneur.App.Schema.Payloads;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.AspNetCore.Identity;

namespace Depanneur.App.Schema.Mutations
{
    public class ProductMutations : ObjectGraphType<int>
    {
        public ProductMutations(ProductRepository products, SubscriptionRepository subscriptions, UserManager<User> userManager)
        {
            Field<ProductType>(
                "update",
                description: "Update the product's details. Requires the 'Products' role.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ProductInputType>> { Name = "product", Description = "The new details to update the product with." }
                ),
                resolve: ctx =>
                {
                    var id = ctx.Source;
                    var data = ctx.GetArgument<ProductInputType.Data>("product");

                    var product = products.Get(id);

                    product.Name = data.Name;
                    product.Description = data.Description;
                    product.Price = data.Price;
                    product.IsSubscribable = data.CanSubscribe;

                    products.Save();

                    return product;
                }
            ).AuthorizeWith(Policies.ManageProducts);

            Field<SubscriptionType>(
                "subscribe",
                description: "Create or update a subscription to this product by the current user.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "quantity", Description = "The quantity to purchase at the given frequency." },
                    new QueryArgument<EnumerationGraphType<SubscriptionFrequency>> { Name = "frequency", Description = "The frequency at which to purchase the product." }
                ),
                resolve: ctx =>
                {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);
                    var productId = ctx.Source;
                    var quantity = ctx.GetArgument<int>("quantity");
                    var frequency = ctx.GetArgument<SubscriptionFrequency>("frequency");

                    return subscriptions.CreateOrUpdate(userId, productId, quantity, frequency);
                }
            );

            Field<UnsubscribePayloadType, UnsubscribePayloadType.Data>("unsubscribe")
                .Description("Unsubscribe the current user from the product.")
                .ResolveAsync(async ctx =>
                {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);
                    var productId = ctx.Source;

                    var sub = await subscriptions.GetProductSubscription(userId, productId);
                    if (sub != null) {
                        subscriptions.Delete(sub);
                    }

                    return new UnsubscribePayloadType.Data
                    {
                        UserId = userId,
                        ProductId = productId
                    };
                });


            Field<ProductType>(
                "delete",
                description: "Mark the product as deleted. Requires the 'Products' role.",
                resolve: ctx => products.DeleteById(ctx.Source)
            ).AuthorizeWith(Policies.ManageProducts);
            Field<ProductType>(
                "restore",
                description: "Unmark the product as deleted. Requires the 'Products' role.",
                resolve: ctx => products.UndeleteById(ctx.Source)
            ).AuthorizeWith(Policies.ManageProducts);
        }
    }
}