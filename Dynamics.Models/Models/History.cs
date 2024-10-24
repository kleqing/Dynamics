using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Models.Models
{
    public class History
    {
        public Guid HistoryID { get; set; }
        public Guid ProjectID { get; set; }
        [MaxLength(100, ErrorMessage = "Phase cannot exceed 100 characters.")]
        public string Phase { get; set; }
        [DataType(DataType.Date)]
        public DateOnly Date { get; set; }
        [MaxLength(1000, ErrorMessage = "Content cannot exceed 1000 characters.")]
        public string Content { get; set; }
        public string? Attachment { get; set; }
        public virtual Project Project { get; set; }
    }
}
