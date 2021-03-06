using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using Service.ClientProfile.Grpc;
using Service.ClientProfile.Grpc.Models.Requests;
using Service.KYC.Client;
using Service.KYC.Domain.Models.Enum;
using Service.KYC.Grpc.Models;
using Service.PersonalData.Grpc;
using Service.PersonalData.Grpc.Contracts;
using Service.Sendgrid.Profile.Domain.Models;
using Service.Sendgrid.Profile.Grpc;
using Service.Sendgrid.Profile.Grpc.Models;
using SimpleTrading.PersonalData.Abstractions.Auth.Consts;

namespace Service.Sendgrid.Profile.Services
{
    public class SendGridProfileService : ISendGridProfileService
    {
        private readonly ILogger<SendGridProfileService> _logger;
        private readonly IPersonalDataServiceGrpc _personalData;
        private readonly IKycStatusClient _kycStatusService;
        private readonly IMyNoSqlServerDataReader<RootSessionNoSqlEntity> _sessionReader;
        private readonly IWalletBalanceService _walletBalanceService;
        private readonly IWalletService _walletService;
        private readonly IClientProfileService _clientProfile;

        private readonly SendGridClient _client;
        private Dictionary<string, string> _fieldsDictionary = new Dictionary<string, string>();

        public SendGridProfileService(ILogger<SendGridProfileService> logger, IPersonalDataServiceGrpc personalData,
            IKycStatusClient kycStatusService, IWalletService walletService,
            IWalletBalanceService walletBalanceService, IMyNoSqlServerDataReader<RootSessionNoSqlEntity> sessionReader, IClientProfileService clientProfile)
        {
            _logger = logger;
            _personalData = personalData;
            _kycStatusService = kycStatusService;
            _walletService = walletService;
            _walletBalanceService = walletBalanceService;
            _sessionReader = sessionReader;
            _clientProfile = clientProfile;

            _client = new SendGridClient(Program.Settings.ApiKey);
        }

        public async Task SubmitProfile(SubmitRequest request)
        {
            if (request.ClientId == SpecialUserIds.EmptyUser.ToString("N"))
                return;

            var profile = await _clientProfile.GetOrCreateProfile(new GetClientProfileRequest()
            {
                ClientId = request.ClientId
            });
            
            var pd = await _personalData.GetByIdAsync(new GetByIdRequest
            {
                Id = request.ClientId
            });

            if (profile == null)
            {
                _logger.LogError($"Cannot find clientProfile for: {request.ClientId}");
                return;
            }

            if (pd.PersonalData == null)
            {
                _logger.LogError($"Cannot find PersonalData for: {request.ClientId}");
                return;
            }
            
            if (pd.PersonalData?.Confirm == null)
            {
                return;
            }

            if (pd.PersonalData?.IsDeactivated == true || profile?.MarketingEmailAllowed == false)
            {
                await DeleteContact(pd.PersonalData.Email, request.ClientId);
                await DeleteContact(pd.PersonalData.Email.Replace("_deactivated", ""), request.ClientId);
                _logger.LogInformation("Remove user from marketing list: {clientId}", request.ClientId);
                return;
            }
            
            var wallet = await _walletService.GetDefaultWalletAsync(new JetClientIdentity
            {
                ClientId = request.ClientId,
                BrandId = pd.PersonalData.BrandId,
                BrokerId = Program.Settings.DefaultBroker
            });

            var balanceResponse = await _walletBalanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest
            {
                WalletId = wallet.WalletId
            });

            var kycProfile = await _kycStatusService.GetClientKycStatus(new KycStatusRequest
            {
                BrokerId = Program.Settings.DefaultBroker,
                ClientId = request.ClientId
            });

            try
            {
                var session = _sessionReader.Get(t => t.TraderId == request.ClientId)?.MaxBy(t => t.CreateTime);

                var requestModel = new ProfileRequestModel
                {
                    AddressLine1 = pd.PersonalData.Address ?? "",
                    City = pd.PersonalData.City  ?? "",
                    Country = pd.PersonalData.CountryOfResidence ?? "",
                    Email = pd.PersonalData.Email ?? "",
                    FirstName = pd.PersonalData.FirstName ?? "",
                    LastName = pd.PersonalData.LastName ?? "",
                    PostalCode = pd.PersonalData.PostalCode ?? "",
                    CustomFields = new CustomFields
                    {
                        RegDate = wallet.CreatedAt.ToString("d"),
                        FirstDeposit = balanceResponse.Balances?.Any().ToString() ?? "False",
                        PhoneVerify = (pd.PersonalData.ConfirmPhone != null).ToString(),
                        KycVerify = GetKycStatus(kycProfile),
                        Earn = wallet.EnableEarnProgram.ToString(),
                        //Country = pd.PersonalData.CountryOfResidence,
                        Lang = "en", //TODO: get lang
                        LastEnter = (session?.CreateTime.Date ?? DateTime.MinValue.Date).ToString("d"),
                        OsType = "Unknown" //TODO: get os
                    }
                };

                var contacts = new ContactsModel
                {
                    Profiles = new List<ProfileRequestModel> {requestModel}
                };

                var contactsJson = contacts.ToJson();
                foreach (var (name, id) in _fieldsDictionary) contactsJson = contactsJson.Replace(name, id);

                var response = await _client.RequestAsync(
                    BaseClient.Method.PUT,
                    urlPath: "marketing/contacts",
                    requestBody: contactsJson
                );

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation(
                        "Client profile {clinetId} submitted to sendgrid with status {status}. Response {response}",
                        request.ClientId, response.StatusCode, response.Body.ReadAsStringAsync().Result);
                else
                    _logger.LogError("Client profile submitted to sendgrid with status {status}. Response {response}",
                        response.StatusCode, response.Body.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                throw;
            }

            //locals
            string GetKycStatus(KycStatusResponse response)
            {
                if (response.VerificationInProgress)
                    return "prog";
                if (response.DepositStatus == KycOperationStatus.Allowed || response.DepositStatus == KycOperationStatus.AllowedWithKycAlert &&
                    response.TradeStatus == KycOperationStatus.Allowed || response.TradeStatus == KycOperationStatus.AllowedWithKycAlert &&
                    response.WithdrawalStatus == KycOperationStatus.Allowed || response.WithdrawalStatus == KycOperationStatus.AllowedWithKycAlert)
                    return "true";

                return "false";
            }

            
            
        }

        private async Task DeleteContact(string email, string clientId)
        {
            var req = new
            {
                query = "email LIKE {email}"
            };
            
            var searchResponse = await _client.RequestAsync(
                BaseClient.Method.POST,
                urlPath: "marketing/contacts/search",
                requestBody: req.ToJson().Replace("{email}", $"'{email}%'"));
            
            if (!searchResponse.IsSuccessStatusCode)
                return;

            var json = await searchResponse.Body.ReadAsStringAsync();

            SearchResponse search = null;
            try
            {
                search = await System.Text.Json.JsonSerializer.DeserializeAsync<SearchResponse>(await searchResponse.Body.ReadAsStreamAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot deserialize search response: {json}; clientId: {clientId}");
                return;
            }

            var id = search?.Result.FirstOrDefault()?.Id;
            if (string.IsNullOrEmpty(id))
                return;

            var queryParams = @"{ 'ids': 'DELETE_ID' }".Replace("DELETE_ID", id);
            var deleteResponse = await _client.RequestAsync(
                BaseClient.Method.DELETE,
                urlPath: "marketing/contacts",
                queryParams: queryParams);
            
            _logger.LogInformation("contact delete response: {statusCode}, clientId: {clientId}", deleteResponse.StatusCode, clientId);
        }
        
        public async Task InitCustomFields()
        {
            await CreateFieldsDictionary();
            var fields = new List<string>();
            var shouldUpdate = false;
            
            if (!_fieldsDictionary.TryGetValue("Reg_date", out _))
                fields.Add(@"{""name"": ""Reg_date"",""field_type"": ""Date""}");
            if (!_fieldsDictionary.TryGetValue("First_deposit", out _))
                fields.Add(@"{""name"": ""First_deposit"",""field_type"": ""Text""}");
            if (!_fieldsDictionary.TryGetValue("Phone_verify", out _))
                fields.Add(@"{""name"": ""Phone_verify"",""field_type"": ""Text""}");
            if (!_fieldsDictionary.TryGetValue("KYC_verify", out _))
                fields.Add(@"{""name"": ""KYC_verify"",""field_type"": ""Text""}");
            if (!_fieldsDictionary.TryGetValue("Earn", out _))
                fields.Add(@"{""name"": ""Earn"",""field_type"": ""Text""}");
            if (!_fieldsDictionary.TryGetValue("Lang", out _))
                fields.Add(@"{""name"": ""Lang"",""field_type"": ""Text""}");
            if (!_fieldsDictionary.TryGetValue("Last_enter", out _))
                fields.Add(@"{""name"": ""Last_enter"",""field_type"": ""Date""}");
            if (!_fieldsDictionary.TryGetValue("OS_type", out _))
                fields.Add(@"{""name"": ""OS_type"",""field_type"": ""Text""}");

            if (fields.Any())
            {               
                shouldUpdate = true;
                foreach (var data in fields)
                {
                    var response = await _client.RequestAsync(
                        BaseClient.Method.POST,
                        urlPath: "marketing/field_definitions",
                        requestBody: data
                    );

                    _logger.LogInformation(
                        "Custom fields created to sendgrid with status {status}. Response {response}",
                        response.StatusCode, response.Body.ReadAsStringAsync().Result);
                }
            }

            if (shouldUpdate)
                await CreateFieldsDictionary();
        }

        private async Task CreateFieldsDictionary()
        {
            _fieldsDictionary = new Dictionary<string, string>();
            var fieldsResponse = await _client.RequestAsync(
                BaseClient.Method.GET,
                urlPath: "marketing/field_definitions"
            );
            if (fieldsResponse.IsSuccessStatusCode)
            {
                var fields = await
                    System.Text.Json.JsonSerializer.DeserializeAsync<CustomFieldsResponse>(
                        await fieldsResponse.Body.ReadAsStreamAsync());
                _fieldsDictionary = fields.CustomFields.ToDictionary(t => t.Name, t => t.Id);
            }
        }
    }
}