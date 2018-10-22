using System.Security.Claims;
using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Helpers;
using Depanneur.App.Models;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Identity;

namespace Depanneur.App.Schema
{
    public class ProductStatisticsType : ObjectGraphType<Product>
    {
        public ProductStatisticsType(DataLoader loader, ProductRepository products, UserManager<User> userManager)
        {
            Name = "ProductStatistics";
            Description = "Statistics about a specific product.";

            Field<NonNullGraphType<IntGraphType>, int>("month")
                .Description("The number of times this product was purchased by the current user in the current month.")
                .ResolveAsync(async ctx => {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);
                    var stats = await loader.LoadBatch("GetPurchaseStatistics", ctx.Source.Id, ids => products.GetPurchaseStatistics(userId, ids));
                    return stats.Month;
                });

            Field<NonNullGraphType<IntGraphType>, int>("total")
                .Description("The total number of times this product was purchased by the current user.")
                .ResolveAsync(async ctx => {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);
                    var stats = await loader.LoadBatch("GetPurchaseStatistics", ctx.Source.Id, ids => products.GetPurchaseStatistics(userId, ids));
                    return stats.Total;
                });

            Field<IntGraphType, int?>("weeklyAverage")
                .Description("The  number of times this product is purchased, per week, by the current user. Only considers the last 3 weeks where the user purchased the product at least once.")
                .ResolveAsync(async ctx => {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);
                    return await loader.LoadBatch("GetWeeklyAverages", ctx.Source.Id, ids => products.GetWeeklyAverages(userId, ids));
                });
        }
    }
}