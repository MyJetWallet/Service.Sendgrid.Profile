using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Service.Sendgrid.Profile.Domain.Models;

public class ListsResponse
{
    [JsonPropertyName("result")]
    public List<ListResult> Result { get; set; }
    
    public class ListResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("contact_count")]
        public int ContactCount { get; set; }
    }
}