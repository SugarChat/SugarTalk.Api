using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.CorsPolicy
{
    public class AllowableCorsOriginsSetting : IConfigurationSetting<string[]>
    {
        public AllowableCorsOriginsSetting(IConfiguration configuration)
        {
            Value = configuration.GetValue<string>("AllowableCorsOrigins").Split(',');
        }

        public string[] Value { get; set; }
    }
}