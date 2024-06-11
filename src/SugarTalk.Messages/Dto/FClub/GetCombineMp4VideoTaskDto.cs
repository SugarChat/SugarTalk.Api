using System;
using Newtonsoft.Json;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.FClub;

public class GetCombineMp4VideoTaskByIdDto
{
    public Guid TaskId { get; set; }
}

public class GetCombineMp4VideosTaskResponse : SugarTalkResponse<GetCombineMp4VideoTaskDto>
{
}

public class GetCombineMp4VideoTaskDto
{
    [JsonProperty("url")]
    public string? Url { get; set; }
}