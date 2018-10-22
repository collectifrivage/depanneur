using GraphQL.Types;

namespace Depanneur.App.Schema.Inputs
{
    public class AdjustmentInputType : InputObjectGraphType<AdjustmentInputType.Data>
    {
        public class Data
        {
            public decimal Amount { get; set; }
            public string Description { get; set; }
        }

        public AdjustmentInputType()
        {
            Name = "AdjustmentInput";
            Description = "Details about a new adjustment to a user's balance.";

            Field(x => x.Amount).Description("The amount to adjust the user's balance by. Use a negative number to decrease the balance.");
            Field(x => x.Description).Description("Justification for the adjustment");
        }
    }
}