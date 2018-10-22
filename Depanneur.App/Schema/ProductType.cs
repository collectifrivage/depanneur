using System.Linq;
using System.Security.Claims;
using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Helpers;
using Depanneur.App.Models;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.AspNetCore.Identity;

namespace Depanneur.App.Schema
{
    public class ProductType : ObjectGraphType<Product>
    {
        public ProductType(DataLoader loader, SubscriptionRepository subscriptions, ProductRepository products, TransactionRepository transactions, UserManager<User> userManager)
        {
            Name = "Product";
            Description = "Information about a product";

            Field(p => p.Id).Description("Unique ID of this product.");
            Field(p => p.Name).Description("The product's name.");
            Field(p => p.Description, nullable: true).Description("The product's description.");
            Field(p => p.Price).Description("The product's unit price.");
            Field(p => p.IsDeleted).Description("Indicates if this product was deleted.");
            Field("canSubscribe", p => p.IsSubscribable).Description("Indicates if this product can be subscribed to.");

            Field<BooleanGraphType>("isSubscribed", 
                resolve: ctx => {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);
                    return loader.LoadBatch("AreProductsSubscribed", ctx.Source.Id, ids => subscriptions.AreProductsSubscribed(userId, ids));
                },
                description: "Indicates if this product is currently subscribed to by the current user.");

            Field<SubscriptionType>("subscription", 
                resolve: ctx => {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);
                    return loader.LoadBatch("GetProductSubscriptions", ctx.Source.Id, ids => subscriptions.GetProductSubscriptions(userId, ids));
                },
                description: "The details of the current subscription to this product by the current user.");

            Field<ProductStatisticsType>("stats", resolve: ctx => ctx.Source, description: "Statistics about this product.");

            Field<PaginationType<PurchaseTransactionType, Purchase>>(
                "purchases",
                description: "The pruchases of this product by all users. Requires the 'Products' role.",
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "page", DefaultValue = 1, Description = "Page number to retrieve." },
                    new QueryArgument<IntGraphType> { Name = "count", DefaultValue = 20, Description = "Number of items per page." }
                ),
                resolve: ctx => {
                    var page = ctx.GetArgument<int>("page");
                    var count = ctx.GetArgument<int>("count");

                    return transactions
                        .GetAll()
                        .PurchasesOfProduct(ctx.Source.Id)
                        .Chronologically()
                        .GetPageAsync(page, count);
                }).AuthorizeWith(Policies.ManageProducts);
        }
    }
}