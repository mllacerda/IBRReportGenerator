using ReportGenerator.Domain.JsonConverters;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReportGenerator.Domain.Models
{
    public class ReportRequest
    {
        [SwaggerSchema("Unique identifier for the report")]
        public string ReportId { get; set; } = string.Empty;

        [SwaggerSchema("URL to receive the generated report via webhook")]
        public string WebhookUrl { get; set; } = string.Empty;

        [SwaggerSchema("Report parameters (Dictionary or object)\n Example = \"{\\\"key1\\\": \\\"value1\\\", \\\"key2\\\": 42}\"")]
        [JsonConverter(typeof(ObjectJsonConverter))]
        public object? Parameters { get; set; } = null;
    }
}
