using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Service.Sendgrid.Profile.Domain.Models;

public class ProfileRequestModel
{
    [JsonPropertyName("address_line_1")]
    public string AddressLine1 { get; set; }
    [JsonPropertyName("address_line_2")]
    public string AddressLine2 { get; set; }
    [JsonPropertyName("city")]
    public string City { get; set; }
    [JsonPropertyName("country")]
    public string Country { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }
    [JsonPropertyName("last_name")]
    public string LastName { get; set; }
    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }
    [JsonPropertyName("state_province_region")]
    public string StateProvinceRegion { get; set; }
    [JsonPropertyName("custom_fields")]
    public CustomFields CustomFields { get; set; }
}

public class ContactsModel
{
    [JsonPropertyName("contacts")]
    public List<ProfileRequestModel> Profiles { get; set; }
}