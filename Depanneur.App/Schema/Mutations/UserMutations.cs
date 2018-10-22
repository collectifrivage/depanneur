using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Schema.Enums;
using Depanneur.App.Schema.Inputs;
using Depanneur.App.Services;
using GraphQL.Authorization;
using GraphQL.Types;

namespace Depanneur.App.Schema.Mutations
{
    public class UserMutations : ObjectGraphType<string>
    {
        public UserMutations(UserRepository users, TransactionService transactionService, UserService userService)
        {
            Field<UserType>(
                "delete",
                description: "Mark the user as deleted. Requires the 'Users' role.",
                resolve: ctx => users.DeleteById(ctx.Source)
            ).AuthorizeWith(Policies.ManageUsers);

            Field<UserType>(
                "restore",
                description: "Unmark the user as deleted. Requires the 'Users' role.",
                resolve: ctx => users.UndeleteById(ctx.Source)
            ).AuthorizeWith(Policies.ManageUsers);

            FieldAsync<UserType>(
                "setPermission",
                description: "Assign or unassign a role to the user. Requires the 'Users' role.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<UserPermissionEnumType>> { Name = "permission", Description = "The role to modify." },
                    new QueryArgument<NonNullGraphType<BooleanGraphType>> { Name = "assigned", Description = "'true' to assign the role, or 'false' to unassign it." }
                ),
                resolve: async ctx => {
                    var userId = ctx.Source;
                    var permission = ctx.GetArgument<UserPermission>("permission");
                    var assigned = ctx.GetArgument<bool>("assigned");

                    var role = permission.GetRoleName();
                    return await userService.SetRole(userId, role, assigned);
                }
            ).AuthorizeWith(Policies.ManageUsers);



            FieldAsync<AdjustmentTransactionType>(
                "recordAdjustment",
                description: "Record an adjustment to the user's balance. Requires the 'Balances' role.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<AdjustmentInputType>> { Name = "adjustment", Description = "Details of the adjustment." }
                ),
                resolve: async ctx =>
                {
                    var userId = ctx.Source;
                    var input = ctx.GetArgument<AdjustmentInputType.Data>("adjustment");

                    return await transactionService.AddAdjustment(userId, input.Amount, input.Description);
                }
            ).AuthorizeWith(Policies.Balances);

            FieldAsync<PaymentTransactionType>(
                "recordPayment",
                description: "Record an payment for the user. Requires the 'Balances' role.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DecimalGraphType>> { Name = "amount", Description = "Amount of the payment." }
                ),
                resolve: async ctx =>
                {
                    var userId = ctx.Source;
                    var amount = ctx.GetArgument<decimal>("amount");

                    return await transactionService.AddPayment(userId, amount);
                }
            ).AuthorizeWith(Policies.Balances);
        }
    }
}