using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.LiveKit.Egress;

public class RemoverParticipantRequestDto : BaseEgressRequestDto
{
    [JsonProperty("room")]
    public string Room { get; set; }

    [JsonProperty("identity")]
    public string Identity { get; set; }
}

public class RemoverParticipantResponseDto
{
}