using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Meeting;

public class AsposeTotalNetSettings : IConfigurationSetting
{
    public AsposeTotalNetSettings(IConfiguration configuration)
    {
        AsposeTotalNetContent = configuration.GetValue<string>("AsposeTotalNetContent");
    }

    public string AsposeTotalNetContent { get; set; }
}