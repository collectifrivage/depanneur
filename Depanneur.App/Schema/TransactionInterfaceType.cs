using System;
using System.Security.Claims;
using Depanneur.App.Data;
using Depanneur.App.Entities;
using Depanneur.App.Helpers;
using Depanneur.App.Models;
using Depanneur.App.Services;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Identity;

namespace Depanneur.App.Schema
{
    public class TransactionInterfaceType : InterfaceGraphType<Transaction>
    {
        public TransactionInterfaceType()
        {
            Name = "TransactionInterface";
            Description = "Details about a transaction.";

            Field(x => x.Id).Description("The transaction ID");
            Field<DateTimeGraphType>("timestamp", description: "The time (in UTC) when this transaction took place.");
            Field(x => x.Amount).Description("The amount of this transaction. Negative numbers indicate a credit.");
            Field(x => x.NewBalance).Description("The user's new balance after this transaction was recorded.");
            Field<UserType>("user", description: "The user associated with this transaction.");
        }

        public static void Implement<T>(ObjectGraphType<T> type, DataLoader loader, UserRepository users) where T : Transaction
        {
            type.Field(x => x.Id).Description("The transaction ID");
            type.Field<DateTimeGraphType>("timestamp", resolve: x => DateTime.SpecifyKind(x.Source.Timestamp, DateTimeKind.Utc), description: "The time (in UTC) when this transaction took place.");
            type.Field(x => x.Amount).Description("The amount of this transaction. Negative numbers indicate a credit.");
            type.Field(x => x.NewBalance).Description("The user's new balance after this transaction was recorded.");
            type.Field<UserType>("user", resolve: ctx => loader.LoadBatch("GetUsersById", ctx.Source.UserId, users.GetUsersById), description: "The user associated with this transaction.");

            type.Interface<TransactionInterfaceType>();
        }
    }

    public class PurchaseTransactionType : ObjectGraphType<Purchase>
    {
        public PurchaseTransactionType(DataLoader loader, UserRepository users, ProductRepository products, UserManager<User> userManager, TransactionService transactionService)
        {
            Name = "Purchase";
            Description = "A product purchase.";

            TransactionInterfaceType.Implement(this, loader, users);

            Field("pricePerUnit", x => x.ProductPrice).Description("The unit price of the product at the time of purchase.");
            Field("itemName", x => x.ProductName).Description("The product's name at the time of purchase.");
            Field(x => x.Quantity).Description("The quantity that was purchased");
            Field("wasFromSubscription", x => x.IsFromSubscription).Description("Indicates if this purchase was triggered by a subscription.");
            Field<BooleanGraphType>(
                "canDelete", 
                description: "Indicates if the purchased can be cancelled.",
                resolve: ctx => {
                    var currentUser = ctx.UserContext.As<DepanneurUserContext>().User;
                    var currentUserId = userManager.GetUserId(currentUser);
                    
                    return transactionService.CanCancel(currentUserId, ctx.Source);
                });
            Field<ProductType>("product", resolve: ctx => loader.LoadBatch("GetProductsById", ctx.Source.ProductId, products.GetProductsById));
        }
    }

    public class PaymentTransactionType : ObjectGraphType<Payment>
    {
        public PaymentTransactionType(DataLoader loader, UserRepository users)
        {
            Name = "Payment";
            Description = "A payment. The amount will be negative.";

            TransactionInterfaceType.Implement(this, loader, users);
        }
    }

    public class AdjustmentTransactionType : ObjectGraphType<Adjustment>
    {
        public AdjustmentTransactionType(DataLoader loader, UserRepository users)
        {
            Name = "Adjustment";
            Description = "An adjustment that was made to a user's balance.";

            TransactionInterfaceType.Implement(this, loader, users);

            Field(x => x.Description).Description("Justification for the adjustment.");
        }
    }
}