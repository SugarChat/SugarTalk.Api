using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.LiveKit.Egress;

public class ListParticipantsRequestDto : BaseEgressRequestDto
{
    [JsonProperty("room")]
    public string Room { get; set; }
}

public class ListParticipantsResponseDto
{
}