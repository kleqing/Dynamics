using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models.ViewModel
{
    public class ReviewProjectDonateRequestVM
    {
        public Guid TransactionID { get; set; }
        public string TypeDonor { get; set; }

        public string Attachments { get; set; }
        [Required(ErrorMessage ="The reason need to provided before deny a transaction")]
        public string ReasonToDeny { get; set; }
    }
}
