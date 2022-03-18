using System.Runtime.Serialization;

namespace Service.Sendgrid.Profile.Grpc.Models
{
    [DataContract]
    public class SubmitRequest
    {
        [DataMember(Order = 1)]
        public string ClientId { get; set; }
    }
}