using Depanneur.App.Helpers;
using GraphQL;
using GraphQL.Types;

namespace Depanneur.App.Schema
{
    public class PaginationType<TGraphType, TSourceType> : ObjectGraphType<Pagination<TSourceType>> where TGraphType : IGraphType
    {
        public PaginationType()
        {
            Name = $"{typeof(TGraphType).GraphQLName()}Pagination";
            Description = $"Pagination info about a list of {typeof(TGraphType).GraphQLName()} nodes.";

            Field(x => x.TotalItems).Description("The total count of items in the list.");
            Field(x => x.PageSize).Description("The number of items per page.");
            Field(x => x.CurrentPage).Description("The current page number");
            Field(x => x.PageCount).Description("The total number of pages");

            Field<ListGraphType<TGraphType>>("items", resolve: ctx => ctx.Source.Items, description: "The list of items on the current page.");
        }
    }
}