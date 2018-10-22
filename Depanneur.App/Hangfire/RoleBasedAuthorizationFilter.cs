using Hangfire.Dashboard;

namespace Depanneur.App.Hangfire
{
    public class RoleBasedAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string role;

        public RoleBasedAuthorizationFilter(string role)
        {
            this.role = role;
        }

        public bool Authorize(DashboardContext context)
        {
            return context.GetHttpContext().User.IsInRole(role);
        }
    }
}