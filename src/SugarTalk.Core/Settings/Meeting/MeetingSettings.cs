using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Meeting;

public class MeetingSettings : IConfigurationSetting
{
    public MeetingSettings(IConfiguration configuration)
    {
        MeetingNumberCapacity = configuration.GetValue<int>("MeetingNumberCapacity");

        MeetingNumberBaseCount = configuration.GetValue<int>("MeetingNumberBaseCount");
    }
    
    public int MeetingNumberCapacity { get; set; }
    
    public int MeetingNumberBaseCount { get; set; }
}