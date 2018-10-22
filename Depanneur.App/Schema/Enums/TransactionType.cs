using GraphQL.Types;

namespace Depanneur.App.Schema.Enums
{
    public enum TransactionType { Purchase, Payment, Adjustment }
    
    public class TransactionTypeEnumType : EnumerationGraphType 
    {
        public TransactionTypeEnumType()
        {
            Name = "TransactionType";
            Description = "The possible types of transactions.";

            AddValue("PURCHASE", "A product purchase.", TransactionType.Purchase);
            AddValue("PAYMENT", "A payment made by a user.", TransactionType.Payment);
            AddValue("ADJUSTMENT", "An adjustment made to a user's balance.", TransactionType.Adjustment);
        }
    }
}