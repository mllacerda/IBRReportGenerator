using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator.Domain.Models
{
    public class ReportRequest
    {
        public string ReportId { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = [];
    }
}
