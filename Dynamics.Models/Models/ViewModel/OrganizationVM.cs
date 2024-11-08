using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models.ViewModel
{
    public class OrganizationVM
    {
        public Guid OrganizationID { get; set; }
        public string OrganizationName { get; set; }
        public string? OrganizationEmail { get; set; }

        public string? OrganizationPhoneNumber { get; set; }
        public string? OrganizationAddress { get; set; }
        public int OrganizationStatus { get; set; }

        public string OrganizationDescription { get; set; }
        public string? OrganizationPictures { get; set; }
        public DateOnly StartTime { get; set; }

        public DateOnly? ShutdownDay { get; set; }

        public User CEO { get; set; }

        public int Projects { get; set; }
        public int Members { get; set; }

        public int joinRequests { get; set; }
        public List<OrganizationMember> OrganizationMember { get; set; }
        public List<Project> Project { get; set; }
        public List<OrganizationResource> OrganizationResource { get; set; }
    }
}
