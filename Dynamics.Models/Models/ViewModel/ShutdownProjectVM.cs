using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models.ViewModel
{
    public class ShutdownProjectVM
    {
        public Guid ProjectID { get; set; }
        [Required]
        public string Reason { get; set; }
    }
}
