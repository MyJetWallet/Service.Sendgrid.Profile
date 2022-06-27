using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.ServiceBus.SessionAudit.Models;
using Service.ClientProfile.Domain.Models;
using Service.ClientWallets.Domain.Models.ServiceBus;
using Service.KYC.Domain.Models.Messages;
using Service.PersonalData.Domain.Models.ServiceBus;
using Service.Sendgrid.Profile.Grpc;
using Service.Sendgrid.Profile.Grpc.Models;

namespace Service.Sendgrid.Profile.Jobs
{
    public class ProfileUpdateNotifier
    {
        private readonly ISendGridProfileService _profileService;

        public ProfileUpdateNotifier(ISubscriber<SessionAuditEvent> sessionSubscriber,
            ISubscriber<KycProfileUpdatedMessage> kycSubscriber, 
            ISubscriber<PersonalDataUpdateMessage> pdSubscriber,
            ISubscriber<ClientWalletUpdateMessage> walletSubscriber,
            ISubscriber<ClientProfileUpdateMessage> profileSubscriber, 
            ISendGridProfileService profileService)
        {
            _profileService = profileService;
            sessionSubscriber.Subscribe(HandleMessages);
            kycSubscriber.Subscribe(HandleMessages);
            pdSubscriber.Subscribe(HandleMessages);
            walletSubscriber.Subscribe(HandleMessages);
            profileSubscriber.Subscribe(HandleMessages);
        }

        private async ValueTask HandleMessages(ClientProfileUpdateMessage message)
        {
            await _profileService.SubmitProfile(new SubmitRequest
            {
                ClientId = message.NewProfile.ClientId
            });        
        }

        private async ValueTask HandleMessages(ClientWalletUpdateMessage message)
        {
            if (message.OldWallet.EnableEarnProgram != message.NewWallet.EnableEarnProgram)
                await _profileService.SubmitProfile(new SubmitRequest
                {
                    ClientId = message.NewWallet.WalletId.Replace("SP-", "")
                });
        }

        private async ValueTask HandleMessages(PersonalDataUpdateMessage message)
        {
            await _profileService.SubmitProfile(new SubmitRequest
            {
                ClientId = message.TraderId
            });
        }

        private async ValueTask HandleMessages(KycProfileUpdatedMessage message)
        {
            await _profileService.SubmitProfile(new SubmitRequest
            {
                ClientId = message.ClientId
            });
        }

        private async ValueTask HandleMessages(SessionAuditEvent message)
        {
            if (message.Action == SessionAuditEvent.SessionAction.Login)
                await _profileService.SubmitProfile(new SubmitRequest
                {
                    ClientId = message.Session.TraderId
                });
        }
    }
}