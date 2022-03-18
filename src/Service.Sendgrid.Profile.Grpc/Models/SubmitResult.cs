using System.Runtime.Serialization;
using Service.Sendgrid.Profile.Domain.Models;

namespace Service.Sendgrid.Profile.Grpc.Models
{
    [DataContract]
    public class SubmitResult : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}