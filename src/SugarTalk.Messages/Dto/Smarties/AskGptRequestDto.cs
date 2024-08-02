using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using Newtonsoft.Json;
using SugarTalk.Messages.Dto.OpenAi;
using SugarTalk.Messages.Enums.OpenAi;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Dto.Smarties;

public class AskGptRequestDto
{
    public OpenAiModel? Model { get; set; }

    public List<CompletionsRequestMessageDto> Messages { get; set; }

    public List<CompletionsRequestFunctionDto> Functions { get; set; }

    [JsonProperty("function_call")] 
    public object FunctionCall { get; set; }

    [JsonProperty("response_format")] 
    public CompletionResponseFormatDto ResponseFormat { get; set; }

    public int? Seed { get; set; }

    public double Temperature { get; set; }

    public TimeSpan? Timeout { get; set; }
}

public class AskGptResponseDto : SugarTalkResponse<CompletionsResponseDto>
{
}