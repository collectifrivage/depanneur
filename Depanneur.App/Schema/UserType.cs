using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Helpers;
using Depanneur.App.Models;
using Depanneur.App.Schema.Enums;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Identity;

namespace Depanneur.App.Schema
{
    public class UserType : ObjectGraphType<User>
    {
        private readonly UserRepository users;

        public UserType(UserManager<User> userManager, UserRepository users, TransactionRepository transactions, DataLoader loader)
        {
            this.users = users;

            Name = "User";
            Description = "Details about a specific user";

            Field(u => u.Id).Description("The user's unique ID.");
            Field(u => u.Name).Description("The user's name");
            Field(u => u.Email).Description("Email address associated with this user.");
            Field(u => u.IsDeleted).Description("Indicates if this user was deleted.");

            FieldAsync<DecimalGraphType>(
                "balance",
                description: "Current balance of this user. Requires the 'Balances' role, or that the user is the current user.",
                resolve: async ctx => {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var currentUserId = userManager.GetUserId(currentUser);

                    if (currentUserId != ctx.Source.Id) await ctx.AuthorizeWith(Policies.Balances);
                    if (ctx.HasError()) return null;

                    return ctx.Source.Balance;
                }
            );
            
            FieldAsync<PaginationType<TransactionInterfaceType, Transaction>>(
                "transactions",
                description: "The list of transactions associated with this user. Requires the 'Balances' role, or that the user is the current user.",
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "page", DefaultValue = 1, Description = "Page number to retrieve." },
                    new QueryArgument<IntGraphType> { Name = "count", DefaultValue = 20, Description = "Number of items per page." },
                    new QueryArgument<ListGraphType<TransactionTypeEnumType>> { Name = "types", DefaultValue = new []{TransactionType.Purchase, TransactionType.Payment, TransactionType.Adjustment}, Description = "The types of transactions to return."}
                ),
                resolve: async ctx => {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var currentUserId = userManager.GetUserId(currentUser);

                    if (ctx.Source.Id != currentUserId) await ctx.AuthorizeWith(Policies.Balances);
                    if (ctx.HasError()) return null;

                    var page = ctx.GetArgument<int>("page");
                    var count = ctx.GetArgument<int>("count");
                    var types = ctx.GetArgument<TransactionType[]>("types");

                    return await transactions
                        .GetAll()
                        .ForUser(ctx.Source.Id)
                        .IncludeTypes(
                            purchases: types.Contains(TransactionType.Purchase), 
                            payments: types.Contains(TransactionType.Payment), 
                            adjustments: types.Contains(TransactionType.Adjustment)
                        )
                        .Chronologically()
                        .GetPageAsync(page, count)
                        .ConfigureAwait(false);
                });

            Field<ListGraphType<UserPermissionEnumType>>(
                "permissions",
                description: "The roles granted to this user.",
                resolve: ctx => loader.LoadBatch("GetUserPermissions", ctx.Source.Id, GetUserPermissions));
        }

        private async Task<IDictionary<string, List<UserPermission>>> GetUserPermissions(IEnumerable<string> ids)
        {
            var result = await users.GetUsersRoles(ids).ConfigureAwait(false);
            return result.ToDictionary(x => x.Key, x => x.Value.Select(GetPermission).ToList());
        }

        private UserPermission GetPermission(string roleName)
        {
            switch (roleName)
            {
                case Roles.Users: return UserPermission.Users;
                case Roles.Products: return UserPermission.Products;
                case Roles.Balances: return UserPermission.Balances;
                default: throw new ArgumentException($"Unknown role: {roleName}", nameof(roleName));
            }
        }
    }
}