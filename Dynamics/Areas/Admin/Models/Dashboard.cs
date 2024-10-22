using Dynamics.Models.Models;
using System.Diagnostics.Metrics;

namespace Dynamics.Areas.Admin.Models
{

    // This class is used to display the dashboard of the admin

    // Reason: Divide data from any action or logic processing.
    // The controller will not directly process the data but will get it from the repository through methods like CountUser(), GetTop5User(),
    // ViewRecentItem(), then wrap it in the model to send to the viewe

    // Easy to maintain and expand: You can easily change the data structure without having to edit many places in the code.
    // For example, if you add a new field to the Dashboard, you only need to edit the model instead of many places in the controller or view.

    public class Dashboard
    {
        public List<User> TopUser { get; set; }
        public List<Organization> TopOrganization { get; set; }
        public List<Request> GetRecentRequest { get; set; }

        // Count
        public int UserCount { get; set; }
        public int OrganizationCount { get; set; }
        public int RequestCount { get; set; }
        public int ProjectCount { get; set; }

    }
}
