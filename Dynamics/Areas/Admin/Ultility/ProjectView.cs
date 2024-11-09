namespace Dynamics.Areas.Admin.Ultility
{
    public class ProjectView
    {
        public static string GetStatusClass(int status)
        {
            return status switch
            {
                -1 => "bg-danger", // Shutdown
                0 => "bg-warning", // Preparing
                1 => "bg-primary", // On going
                2 => "bg-success",  // Finished
                _ => "bg-secondary" // Default
            };
        }

        public static string GetStatusText(int status)
        {
            return status switch
            {
                -1 => "Canceled",
                0 => "Preparing",
                1 => "On-going",
                2 => "Finished",
                _ => "Unknown"
            };
        }
    }
}
