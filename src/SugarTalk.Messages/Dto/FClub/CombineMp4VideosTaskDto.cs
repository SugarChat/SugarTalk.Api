using System;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.FClub;

public class CombineMp4VideosTaskDto : CombineMp4VideosDto
{
    public Guid? TaskId { get; set; }
}

public class CombineMp4VideosTaskResponse : SugarTalkResponse<Guid>
{
}