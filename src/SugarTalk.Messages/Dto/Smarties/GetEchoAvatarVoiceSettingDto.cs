using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.Smarties;

public class GetEchoAvatarVoiceSettingRequestDto
{
    public string UserName { get; set; }
    
    public Guid VoiceUuid { get; set; }
    
    public EchoAvatarLanguageType LanguageType { get; set; }
}

public class GetEchoAvatarUserToneResponse : SugarTalkResponse<EchoAvatarUserToneDto>
{
}

public class EchoAvatarUserToneDto
{
    public EchoAvatarVoiceDto EchoAvatarVoice { get; set; }
    
    public List<EchoAvatarInferenceRecordDto> InferenceRecords { get; set; }
}

public class EchoAvatarVoiceDto
{
    public int Id { get; set; }
    
    public int EchoAvatarUserId { get; set; }
    
    [JsonProperty("UUID")]
    public Guid Uuid { get; set; }
    
    public string Name { get; set; }
    
    public int Epochs { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
}

public class EchoAvatarInferenceRecordDto
{
    public int Id { get; set; }
    
    public int VoiceId { get; set; }
    
    public int TextId { get; set; }
    
    public int EchoAvatarUserId { get; set; }
    
    public float Transpose { get; set; }
    
    public float Speed { get; set; }
    
    public int Style { get; set; }
    
    public EchoAvatarLanguageType Language { get; set; }
    
    public bool IsDefault { get; set; }
    
    public string AuditionUrl { get; set; }
    
    public string Note { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
    
    public DateTimeOffset UpdatedDate { get; set; }
}