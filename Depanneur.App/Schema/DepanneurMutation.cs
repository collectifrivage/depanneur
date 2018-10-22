using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Models;
using Depanneur.App.Schema.Inputs;
using Depanneur.App.Schema.Mutations;
using Depanneur.App.Schema.Payloads;
using Depanneur.App.Services;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.AspNetCore.Identity;

namespace Depanneur.App.Schema
{
    public class DepanneurMutation : ObjectGraphType
    {
        public DepanneurMutation(UserRepository users, ProductRepository products, TransactionService transactionService, UserManager<User> userManager)
        {
            Description = "The root mutation type.";

            FieldAsync<PurchaseTransactionType>(
                "purchase",
                description: "Records a purchase for the current user.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> {Name = "productId", Description = "The ID of the product to purchase."}
                ),
                resolve: async ctx =>
                {
                    var productId = ctx.GetArgument<int>("productId");
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);

                    return await transactionService.PurchaseProduct(userId, productId, 1);
                }
            );

            FieldAsync<BatchCancellationResultType>(
                "cancelPurchases",
                description: "Cancels the specified purchases, provided they are for the current user and within the allowed cancellation period.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<IntGraphType>>>> { Name = "purchaseIds", Description = "The list of IDs of the purchases to cancel." }
                ),
                resolve: async ctx => 
                {
                    var purchaseIds = ctx.GetArgument<int[]>("purchaseIds");
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var userId = userManager.GetUserId(currentUser);

                    var deletedPurchases = await transactionService.CancelPurchases(userId, purchaseIds);

                    return new BatchCancellationResultType.Data
                    {
                        User = users.Get(userId),
                        Products = (await products.GetProductsById(deletedPurchases.Select(p => p.ProductId))).Values
                    };
                }
            );

            Field<ProductMutations>(
                "product",
                description: "Gives access to mutations on a specific product.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "id", Description = "The ID of the product to mutate." }
                ),
                resolve: ctx => ctx.GetArgument<int>("id")
            );
            Field<ProductType>(
                "addProduct",
                description: "Adds a new product. Requires the 'Products' role.",
                arguments: new QueryArguments(
                    new QueryArgument<ProductInputType> { Name = "product", Description = "The details of the product to add." }
                ),
                resolve: ctx =>
                {
                    var input = ctx.GetArgument<ProductInputType.Data>("product");
                    return products.Add(new Product
                    {
                        Name = input.Name,
                        Description = input.Description,
                        Price = input.Price,
                        IsSubscribable = input.CanSubscribe
                    });
                }
            ).AuthorizeWith(Policies.ManageProducts);

            Field<UserMutations>(
                "user",
                description: "Gives access to mutations on a specific user.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The ID of the user to mutate." }
                ),
                resolve: ctx => ctx.GetArgument<string>("id")
            );
            FieldAsync<UserType>(
                "addUser",
                description: "Adds a new user. Requires the 'Users' role.",
                arguments: new QueryArguments(
                    new QueryArgument<UserInputType> { Name = "user", Description = "The details of the user to add." }
                ),
                resolve: async ctx =>
                {
                    var input = ctx.GetArgument<UserInputType.Data>("user");
                    var user = new User
                    {
                        Name = input.Name,
                        Email = input.Email,
                        UserName = input.Email
                    };

                    var result = await userManager.CreateAsync(user);

                    if (result.Succeeded)
                        return user;

                    throw new System.Exception(result.Errors.First().Description);
                }
            ).AuthorizeWith(Policies.ManageUsers);
        }
    }
}