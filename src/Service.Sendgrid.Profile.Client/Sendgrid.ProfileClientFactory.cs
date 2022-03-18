using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Sendgrid.Profile.Grpc;

namespace Service.Sendgrid.Profile.Client
{
    [UsedImplicitly]
    public class SendgridProfileClientFactory: MyGrpcClientFactory
    {
        public SendgridProfileClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public ISendGridProfileService GetHelloService() => CreateGrpcService<ISendGridProfileService>();
    }
}
