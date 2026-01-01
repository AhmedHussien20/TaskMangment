using global::Hangfire.Dashboard;

namespace TaskMangment.Hangfire
{ 
    public class HangfireAllowAllFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
        
    }


}
