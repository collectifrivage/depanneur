using GraphQL;
using GraphQL.Types;

namespace Depanneur.App.Schema.Inputs
{
    public class ProductInputType : InputObjectGraphType<ProductInputType.Data>
    {
        public class Data
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public bool CanSubscribe { get; set; }
        }

        public ProductInputType()
        {
            Name = "ProductInput";
            Description = "Details about a product.";

            Field(x => x.Name).Description("The product's name.");
            Field(x => x.Description, nullable: true).Description("The product's description.");
            Field(x => x.Price).Description("The product's unit price.");
            Field(x => x.CanSubscribe).Description("Whether or not the product should be allowed to be subscribed to.");
        }
    }
}