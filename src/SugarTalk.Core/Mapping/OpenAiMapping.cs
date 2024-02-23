using AutoMapper;
using OpenAI.ObjectModels.ResponseModels;
using SugarTalk.Messages.Enums.OpenAi;

namespace SugarTalk.Core.Mapping;

public class OpenAiMapping : Profile
{
    public OpenAiMapping()
    {
        CreateMap<AudioCreateTranscriptionResponse, AudioTranscriptionResponseDto>();
    }
}