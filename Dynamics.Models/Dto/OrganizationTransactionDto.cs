using Dynamics.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Dto
{
    public class OrganizationTransactionDto
    {
        public Guid TransactionID { get; set; }
        public string Name { get; set; }
        public Project? Project { get; set; }
        public string Unit { get; set; }
        public int Amount { get; set; }
        public string ResourceName { get; set; }
        public string? Message { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateOnly Time { get; set; }
        public int Status { get; set; }
        public string Attachments { get; set; }
        public string Type { get; set; }
    }
}
