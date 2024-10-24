using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Dto
{
    
    public class UpdateProjectProfileRequestDto
    {
        [ValidateNever]
        public Guid ProjectID { get; set; }
        [ValidateNever]
        public string ProjectName { get; set; }
        [ValidateNever]
        public int ProjectStatus { get; set; }
        [ValidateNever]
        public string? Attachment { get; set; }
        [ValidateNever]
        public string? ProjectDescription { get; set; }
        [DataType(DataType.Date)]
        [ValidateNever]
        public DateOnly? StartTime { get; set; }

        [DataType(DataType.Date)]
        [ValidateNever]
        public DateOnly? EndTime { get; set; }
        [ValidateNever]
        public Guid NewLeaderID { get; set; }
        [ValidateNever]
        public Guid OldLeaderID { get; set; }
        [DataType(DataType.PhoneNumber)]
        [ValidateNever]
        public string? ProjectPhoneNumber { get; set; }
        [ValidateNever]
        public string? ProjectAddress { get; set; }
        [ValidateNever]
        [DataType(DataType.EmailAddress)]
        public string ProjectEmail { get; set; }
        [ValidateNever]
        public string? ReportFile { get; set; }
    }
}
