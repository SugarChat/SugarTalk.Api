using Newtonsoft.Json;
using System.Collections.Generic;

namespace SugarTalk.Messages.Dto.LiveKit.Egress;

public class GetEgressInfoListResponseDto
{
    [JsonProperty("items")]
    public List<EgressItemDto> EgressItems { get; set; }
}

public class GetEgressRequestDto : BaseEgressRequestDto
{
}