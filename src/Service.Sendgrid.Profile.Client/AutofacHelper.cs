using Autofac;
using Service.Sendgrid.Profile.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Sendgrid.Profile.Client
{
    public static class AutofacHelper
    {
        public static void RegisterSendgridProfileClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new SendgridProfileClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<ISendGridProfileService>().SingleInstance();
        }
    }
}
