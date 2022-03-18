using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Sendgrid.Profile.Settings
{
    public class SettingsModel
    {
        [YamlProperty("SendgridProfile.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("SendgridProfile.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("SendgridProfile.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
        
        [YamlProperty("SendgridProfile.DefaultBroker")]
        public string DefaultBroker { get; set; }

        [YamlProperty("SendgridProfile.ApiKey")]
        public string ApiKey { get; set; }
        
        [YamlProperty("SendgridProfile.PersonalDataGrpcServiceUrl")]
        public string PersonalDataGrpcServiceUrl { get; set; }
        
        [YamlProperty("SendgridProfile.KycGrpcServiceUrl")]
        public string KycGrpcServiceUrl { get; set; }
        
        [YamlProperty("SendgridProfile.BalancesGrpcServiceUrl")]
        public string BalancesGrpcServiceUrl { get; set; }
        
        [YamlProperty("SendgridProfile.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }
        
        [YamlProperty("SendgridProfile.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }
        
        [YamlProperty("SendgridProfile.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
    }
}
