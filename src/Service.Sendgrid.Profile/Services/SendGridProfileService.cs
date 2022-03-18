using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Authorization.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.WalletApi.Wallets;
using MyNoSqlServer.Abstractions;
using SendGrid;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.KYC.Client;
using Service.KYC.Domain.Models.Enum;
using Service.KYC.Grpc;
using Service.KYC.Grpc.Models;
using Service.PersonalData.Grpc;
using Service.PersonalData.Grpc.Contracts;
using Service.Sendgrid.Profile.Domain.Models;
using Service.Sendgrid.Profile.Grpc;
using Service.Sendgrid.Profile.Grpc.Models;

namespace Service.Sendgrid.Profile.Services
{
    public class SendGridProfileService: ISendGridProfileService
    {
        private readonly ILogger<SendGridProfileService> _logger;
        private readonly IPersonalDataServiceGrpc _personalData;
        private readonly IKycStatusClient _kycStatusService;
        private readonly IMyNoSqlServerDataReader<RootSessionNoSqlEntity> _sessionReader;
        private readonly IWalletBalanceService _walletBalanceService;
        private readonly IWalletService _walletService;
        private readonly SendGridClient _sendGridClient;

        public SendGridProfileService(ILogger<SendGridProfileService> logger, IPersonalDataServiceGrpc personalData,
            IKycStatusClient kycStatusService, IWalletService walletService,
            IWalletBalanceService walletBalanceService, IMyNoSqlServerDataReader<RootSessionNoSqlEntity> sessionReader)
        {
            _logger = logger;
            _personalData = personalData;
            _kycStatusService = kycStatusService;
            _walletService = walletService;
            _walletBalanceService = walletBalanceService;
            _sessionReader = sessionReader;

            _sendGridClient = new SendGridClient(Program.Settings.ApiKey);
        }

        public async Task SubmitProfile(SubmitRequest request)
        {
            var pd = await _personalData.GetByIdAsync(new GetByIdRequest()
            {
                Id = request.ClientId
            });
            
            if(string.IsNullOrWhiteSpace(pd.PersonalData.FirstName))
                return;

            var wallet = await _walletService.GetDefaultWalletAsync(new JetClientIdentity
            {
                ClientId = request.ClientId,
                BrandId = pd.PersonalData.BrandId,
                BrokerId = Program.Settings.DefaultBroker
            });
            
            var balanceResponse = await _walletBalanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = wallet.WalletId
            });

            var kycProfile = _kycStatusService.GetClientKycStatus(new KycStatusRequest()
            {
                BrokerId = Program.Settings.DefaultBroker,
                ClientId = request.ClientId
            });

            var session = _sessionReader.Get(t => t.TraderId == request.ClientId)?.OrderByDescending(t=>t.CreateTime).First();
            var requestModel = new ProfileRequestModel
            {
                AddressLine1 = pd.PersonalData.Address,
                City = pd.PersonalData.City,
                Country = pd.PersonalData.CountryOfResidence,
                Email = pd.PersonalData.Email,
                FirstName = pd.PersonalData.FirstName,
                LastName = pd.PersonalData.LastName,
                PostalCode = pd.PersonalData.PostalCode,
                CustomFields = new CustomFields
                {
                    RegDate = wallet.CreatedAt,
                    FirstDeposit = balanceResponse.Balances.Any(),
                    PhoneVerify = pd.PersonalData.ConfirmPhone != null,
                    KycVerify = GetKycStatus(kycProfile),
                    Earn = wallet.EnableEarnProgram,
                    Country = pd.PersonalData.CountryOfResidence,
                    Lang = "en", //TODO: get lang
                    LastEnter = session?.CreateTime ?? DateTime.MinValue,
                    OsType = null, //TODO: get os
                }
            };
            
            var response = await _sendGridClient.RequestAsync(
                method: SendGridClient.Method.POST,
                urlPath: "marketing/field_definitions",
                requestBody: requestModel.ToJson()
            );

            _logger.LogInformation("Client profile submitted to sendgrid with status {status}. Response {response}", response.StatusCode, response.Body.ReadAsStringAsync().Result);
            //locals
            string GetKycStatus(KycStatusResponse response)
            {
                if (response.VerificationInProgress)
                    return "prog";
                if (response.DepositStatus != KycOperationStatus.Allowed ||
                    response.DepositStatus != KycOperationStatus.AllowedWithKycAlert &&
                    response.TradeStatus != KycOperationStatus.Allowed ||
                    response.TradeStatus != KycOperationStatus.AllowedWithKycAlert &&
                    response.WithdrawalStatus != KycOperationStatus.Allowed ||
                    response.WithdrawalStatus != KycOperationStatus.AllowedWithKycAlert)
                    return "false";

                return "true";
            }
        }

        public async Task InitCustomFields()
        {
            var fields = new List<string>()
            {
                @"{""name"": ""Reg_date"",""field_type"": ""Text""}",
                @"{""name"": ""Phone_verify"",""field_type"": ""Text""}",
                @"{""name"": ""KYC_verify"",""field_type"": ""Text""}",
                @"{""name"": ""Earn"",""field_type"": ""Text""}",
                //@"{""name"": ""Country"",""field_type"": ""Text""}",
                @"{""name"": ""Lang"",""field_type"": ""Text""}",
                @"{""name"": ""Last_enter"",""field_type"": ""Date""}",
                @"{""name"": ""OS_type"",""field_type"": ""Text""}",
            };

            foreach (var data in fields)
            {
                var response = await _sendGridClient.RequestAsync(
                    method: SendGridClient.Method.POST,
                    urlPath: "marketing/field_definitions",
                    requestBody: data
                );

                _logger.LogInformation("Custom fields created to sendgrid with status {status}. Response {response}",
                    response.StatusCode, response.Body.ReadAsStringAsync().Result);
            }
        }
    }
}
