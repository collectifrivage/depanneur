using System.Linq;
using System.Threading.Tasks;
using Depanneur.App.Models;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;

namespace Depanneur.App.Helpers
{
    public static class GraphQLExtensions
    {
        public static async Task<bool> AuthorizeWith<T>(this ResolveFieldContext<T> context, string policyName) 
        {
            var schema = (GraphQL.Types.Schema) context.Schema;
            var evaluator = schema.DependencyResolver.Resolve<IAuthorizationEvaluator>();
            
            var userContext = context.UserContext as DepanneurUserContext;

            var result = await evaluator.Evaluate(userContext.User, userContext, context.Arguments, new[]{policyName});
            if (result.Succeeded) return true;

            context.Errors.AddRange(result.Errors.Select(error => new ExecutionError(error)));
            return false;
        }

        public static bool HasError<T>(this ResolveFieldContext<T> context) 
        {
            return context.Errors.Any();
        }
    }
}