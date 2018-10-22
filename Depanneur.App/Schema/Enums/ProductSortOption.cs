using GraphQL.Types;

namespace Depanneur.App.Schema.Enums
{
    public enum ProductSortOption { Alpha, Usage }

    public class ProductSortEnumType : EnumerationGraphType
    {
        public ProductSortEnumType()
        {
            Name = "ProductSortOption";
            Description = "The different ways to sort a list of products.";

            AddValue("ALPHA", "Sort alphabetically by product name", ProductSortOption.Alpha);
            AddValue("USAGE", "Sort according to total purchases of the products by the current user", ProductSortOption.Usage);
        }
    }
}