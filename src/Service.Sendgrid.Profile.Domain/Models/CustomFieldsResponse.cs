using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Service.Sendgrid.Profile.Domain.Models;

public class CustomFieldsResponse
{
    
    [JsonPropertyName("custom_fields")]
    public List<CustomField> CustomFields { get; set; }
    
    public class CustomField
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("field_type")]
        public string FieldType { get; set; }
    }
}