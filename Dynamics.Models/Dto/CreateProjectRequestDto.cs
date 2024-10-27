using Dynamics.Models.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Dto
{
    public class CreateProjectRequestDto
    {
        [ValidateNever]
        public Project Project { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> OrganizationList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> RequestList { get; set; }
    }
}
