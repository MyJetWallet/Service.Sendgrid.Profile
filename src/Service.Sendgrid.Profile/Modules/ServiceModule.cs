using Autofac;
using MyJetWallet.Sdk.Authorization.NoSql;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using MyJetWallet.Sdk.WalletApi.Wallets;
using MyServiceBus.Abstractions;
using Service.Balances.Client;
using Service.ClientProfile.Client;
using Service.ClientWallets.Client;
using Service.ClientWallets.Domain.Models.ServiceBus;
using Service.KYC.Client;
using Service.KYC.Domain.Models.Messages;
using Service.PersonalData.Client;
using Service.PersonalData.Domain.Models.ServiceBus;
using Service.Sendgrid.Profile.Grpc;
using Service.Sendgrid.Profile.Jobs;
using Service.Sendgrid.Profile.Services;

namespace Service.Sendgrid.Profile.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient =
                builder.RegisterMyServiceBusTcpClient((() => Program.Settings.SpotServiceBusHostPort), Program.LogFactory);
            var queueName = "Service.SendgridProfile";
            builder.RegisterMyServiceBusSubscriberSingle<SessionAuditEvent>(serviceBusClient,
                SessionAuditEvent.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<KycProfileUpdatedMessage>(serviceBusClient,
                KycProfileUpdatedMessage.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<PersonalDataUpdateMessage>(serviceBusClient,
                PersonalDataUpdateMessage.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<ClientWalletUpdateMessage>(serviceBusClient,
                ClientWalletUpdateMessage.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            
            var myNoSqlClient = builder.CreateNoSqlClient(() => Program.Settings.MyNoSqlReaderHostPort);
            builder.RegisterMyNoSqlReader<RootSessionNoSqlEntity>(myNoSqlClient, RootSessionNoSqlEntity.TableName);
            
            builder.RegisterKycStatusClients(myNoSqlClient, Program.Settings.KycGrpcServiceUrl);
            builder.RegisterPersonalDataClient(Program.Settings.PersonalDataGrpcServiceUrl);
            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, myNoSqlClient);
            builder.RegisterClientWalletsClients(myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);
            builder.RegisterClientProfileClients(myNoSqlClient, Program.Settings.ClientProfileGrpcServiceUrl);
            
            builder
                .RegisterType<WalletService>()
                .As<IWalletService>()
                .SingleInstance();

            builder.RegisterType<ProfileUpdateNotifier>().AsSelf().SingleInstance().AutoActivate();
            builder.RegisterType<SendGridProfileService>().As<ISendGridProfileService>().SingleInstance().AutoActivate();
        }
    }
}