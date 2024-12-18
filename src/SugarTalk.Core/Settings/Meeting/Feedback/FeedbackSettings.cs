using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Meeting.Feedback;

public class FeedbackSettings : IConfigurationSetting
{
    public FeedbackSettings(IConfiguration configuration)
    {
        AccountName = configuration.GetValue<string>("Feedback:AccountName").Split(",").ToList();
    }

    public List<string> AccountName { get; set; }
}