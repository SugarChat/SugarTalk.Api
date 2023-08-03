using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Meeting;

public class MeetingSettings : IConfigurationSetting
{
    public MeetingSettings(IConfiguration configuration)
    {
        MeetingNumberCapacity = configuration.GetValue<int>("MeetingNumberCapacity");

        MeetingNumberBaseValue = configuration.GetValue<int>("MeetingNumberBaseValue");
    }
    
    public int MeetingNumberCapacity { get; set; }
    
    public int MeetingNumberBaseValue { get; set; }
}