using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace Dynamics.Areas.Admin.Ultility
{
    public class ReportView
    {
        public static string GetStatusClass(string type)
        {
            return type switch
            {
                "User" => "bg-secondary",
                "Project" => "bg-success",
                "Organization" => "bg-info",  
                _ => "bg-secondary" // Default
            };
        }

        public static string GetStatusText(string type)
        {
            return type switch
            {
                "User" => "User",
                "Project" => "Project",
                "Organization" => "Organization",
                "Users" => "User",
                "Projects" => "Project",
                "Organizations" => "Organization",
                _ => "Unknown"
            };
        }

        public static string GetLinkByType(string type, IUrlHelper Url)
        {
            return type switch
            {
                "User" => Url.Action("Index", "Users", new { area = "Admin" }),
                "Project" => Url.Action("Index", "Projects", new { area = "Admin" }),
                "Organization" => Url.Action("Index", "Organizations", new { area = "Admin" }),
                _ => "#"
            };
        }
    }
}
