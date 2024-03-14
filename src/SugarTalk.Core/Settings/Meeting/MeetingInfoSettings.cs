using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Meeting;

public class MeetingInfoSettings : IConfigurationSetting
{
    public MeetingInfoSettings(IConfiguration configuration)
    {
        InviteBaseUrl = configuration.GetValue<string>("Meeting:Invite:BaseUrl");
    }

    public string InviteBaseUrl { get; set; }
}