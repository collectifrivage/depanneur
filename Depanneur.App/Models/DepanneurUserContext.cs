using System.Security.Claims;
using GraphQL.Authorization;

namespace Depanneur.App.Models
{
    public class DepanneurUserContext : IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }
    }
}