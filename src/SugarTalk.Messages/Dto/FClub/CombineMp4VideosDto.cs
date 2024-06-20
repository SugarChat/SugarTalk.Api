using System.Collections.Generic;
using Newtonsoft.Json;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.FClub;

public class CombineMp4VideosDto
{
    [JsonProperty("filepath")]
    public string filePath { get; set; }

    [JsonProperty("urls")]
    public List<string> urls { get; set; }
}

public class CombineMp4VideosResponse : SugarTalkResponse<string>
{
}