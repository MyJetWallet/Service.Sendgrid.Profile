using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Service.Sendgrid.Profile.Domain.Models;

public class SearchResponse
{
    [JsonPropertyName("result")]
    public List<SearchResult> Result { get; set; }
    
    public class CustomFields
    {
        [JsonPropertyName("Earn")]
        public string Earn { get; set; }

        [JsonPropertyName("First_deposit")]
        public string FirstDeposit { get; set; }

        [JsonPropertyName("KYC_verify")]
        public string KYCVerify { get; set; }

        [JsonPropertyName("Lang")]
        public string Lang { get; set; }

        [JsonPropertyName("Last_enter")]
        public DateTime LastEnter { get; set; }

        [JsonPropertyName("OS_type")]
        public string OSType { get; set; }

        [JsonPropertyName("Phone_verify")]
        public string PhoneVerify { get; set; }

        [JsonPropertyName("Reg_date")]
        public DateTime RegDate { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("self")]
        public string Self { get; set; }
    }

    public class SearchResult
    {
        [JsonPropertyName("address_line_1")]
        public string AddressLine1 { get; set; }

        [JsonPropertyName("address_line_2")]
        public string AddressLine2 { get; set; }

        [JsonPropertyName("alternate_emails")]
        public List<object> AlternateEmails { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("list_ids")]
        public List<object> ListIds { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("segment_ids")]
        public object SegmentIds { get; set; }

        [JsonPropertyName("state_province_region")]
        public string StateProvinceRegion { get; set; }

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("whatsapp")]
        public string Whatsapp { get; set; }

        [JsonPropertyName("line")]
        public string Line { get; set; }

        [JsonPropertyName("facebook")]
        public string Facebook { get; set; }

        [JsonPropertyName("unique_name")]
        public string UniqueName { get; set; }

        [JsonPropertyName("custom_fields")]
        public CustomFields CustomFields { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("_metadata")]
        public Metadata Metadata { get; set; }
    }
    
}