using System.Linq;
using System.Security.Claims;
using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Helpers;
using Depanneur.App.Models;
using Depanneur.App.Schema.Enums;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.AspNetCore.Identity;

namespace Depanneur.App.Schema
{
    public class DepanneurQuery : ObjectGraphType
    {
        public DepanneurQuery(UserManager<User> userManager, ProductRepository products, UserRepository users, AuthorizationSettings gqlAuthSettings, IAuthorizationEvaluator eval)
        {
            Field<ProductType>(
                "product",
                description: "Finds a single product by ID.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "id", Description = "The ID of the product." }
                ),
                resolve: ctx => products.Get(ctx.GetArgument<int>("id"))
            );
            
            FieldAsync<ListGraphType<ProductType>>(
                "products",
                description: "All products, optionally including deleted products.",
                arguments: new QueryArguments(
                    new QueryArgument<BooleanGraphType> { Name = "includeDeleted", DefaultValue = false, Description = "Set to true to include deleted products in the results." },
                    new QueryArgument<ProductSortEnumType>
                    {
                        Name = "sort",
                        DefaultValue = ProductSortOption.Alpha,
                        Description = "Indicates how results are sorted."
                    }
                ),
                resolve: async ctx => {
                    var includeDeleted = ctx.GetArgument<bool>("includeDeleted");
                    var sort = ctx.GetArgument<ProductSortOption>("sort");
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);

                    if (includeDeleted) await ctx.AuthorizeWith(Policies.ManageProducts);
                    if (ctx.HasError()) return null;

                    var query = includeDeleted
                        ? products.GetAll()
                        : products.GetActive();

                    query = sort == ProductSortOption.Usage
                        ? query.SortedByTotalUsage(userId)
                        : query.OrderBy(x => x.Name);

                    return query.ToList();
                }
            );

            Field<UserType>(
                "me",
                description: "Gets the current user.",
                resolve: ctx =>
                {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var id = userManager.GetUserId(currentUser);

                    return users.Get(id);
                }
            );

            Field<UserType>(
                "user",
                description: "Gets a specific user by it's ID.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The ID of the user." }
                ),
                resolve: ctx =>
                {
                    var id = ctx.GetArgument<string>("id");
                    return users.Get(id);
                }
            ).AuthorizeWith(Policies.ReadUsers);

            Field<ListGraphType<UserType>>(
                "users",
                description: "All users, optionnaly including deleted users.",
                arguments: new QueryArguments(
                    new QueryArgument<BooleanGraphType> { Name = "includeDeleted", DefaultValue = false, Description = "Set to true to include deleted users in the results." }
                ),
                resolve: ctx => ctx.GetArgument<bool>("includeDeleted")
                    ? users.GetAll().OrderBy(x => x.Name).ToList()
                    : users.GetActive().OrderBy(x => x.Name).ToList()
            ).AuthorizeWith(Policies.ReadUsers);
        }
    }
}