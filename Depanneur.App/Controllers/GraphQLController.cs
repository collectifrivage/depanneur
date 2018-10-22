using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Depanneur.App.Models;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Depanneur.App.Controllers
{
    [Route("[controller]"), Authorize]
    public class GraphQLController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] GraphQLQuery query, 
            [FromServices] ISchema schema, 
            [FromServices] IDocumentExecuter executer, 
            [FromServices] DataLoaderDocumentListener dataLoaderListener,
            [FromServices] IConfiguration configuration,
            [FromServices] IEnumerable<IValidationRule> rules)
        {
            if (query == null) { throw new ArgumentNullException(nameof(query)); }
            
            var result = await executer.ExecuteAsync(x => {
                x.Schema = schema;
                x.Query = query.Query;
                x.Inputs = query.Variables.ToInputs();
                x.Listeners.Add(dataLoaderListener);
                x.UserContext = new DepanneurUserContext {
                    User = User
                };
                x.ValidationRules = rules;
                
                x.ExposeExceptions = configuration.GetValue<bool>("GraphQL:ExposeExceptions");
            });

            if (result.Errors?.Count > 0)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
    
    public class GraphQLQuery
    {
        public string OperationName { get; set; }
        public string NamedQuery { get; set; }
        public string Query { get; set; }
        public JObject Variables { get; set; } //https://github.com/graphql-dotnet/graphql-dotnet/issues/389
    }
}