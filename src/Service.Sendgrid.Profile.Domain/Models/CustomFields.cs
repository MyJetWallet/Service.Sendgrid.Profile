using System;
using System.Text.Json.Serialization;

namespace Service.Sendgrid.Profile.Domain.Models;

public class CustomFields
{
    [JsonPropertyName("Reg_date")]
    public DateTime RegDate { get; set; }
    [JsonPropertyName("First_deposit")]
    public bool FirstDeposit { get; set; }
    [JsonPropertyName("Phone_verify")]
    public bool PhoneVerify { get; set; }
    [JsonPropertyName("KYC_verify")]
    public string KycVerify { get; set; }
    [JsonPropertyName("Earn")]
    public bool Earn { get; set; }
    // [JsonPropertyName("Country")]
    // public string Country { get; set; }
    [JsonPropertyName("Lang")]
    public string Lang { get; set; }
    [JsonPropertyName("Last_enter")]
    public DateTime LastEnter { get; set; }
    [JsonPropertyName("OS_type")]
    public string OsType { get; set; }

}