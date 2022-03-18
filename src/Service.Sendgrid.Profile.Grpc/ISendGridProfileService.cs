using System.ServiceModel;
using System.Threading.Tasks;
using Service.Sendgrid.Profile.Grpc.Models;

namespace Service.Sendgrid.Profile.Grpc
{
    [ServiceContract]
    public interface ISendGridProfileService
    {
        [OperationContract]
        Task SubmitProfile(SubmitRequest request);
        
        [OperationContract]
        Task InitCustomFields();

    }
}