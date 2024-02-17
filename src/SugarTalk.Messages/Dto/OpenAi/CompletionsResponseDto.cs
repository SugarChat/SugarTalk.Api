using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.OpenAi;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Messages.Dto.OpenAi;

public class CompletionsResponseDto
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("created")]
    public int Created { get; set; }

    [JsonIgnore]
    public DateTime CreatedDate => DateTimeOffset.FromUnixTimeSeconds(Created).DateTime;

    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonProperty("object")]
    public string Object { get; set; }
    
    [JsonProperty("choices")]
    public List<CompletionsChoiceDto> Choices { get; set; } = new();

    [JsonProperty("usage")]
    public CompletionsUsageDto Usage { get; set; }
    
    [JsonProperty("error")]
    public CompletionsErrorDto Error { get; set; }

    [JsonIgnore]
    [NotMapped]
    public OpenAiProvider Provider { get; set; }
    
    [JsonIgnore]
    [NotMapped]
    public ChatCompletionsRequestDto ChatCompletionsRequest { get; set; }
    
    [NotMapped]
    public string Response
    {
        get
        {
            if (Error != null && !string.IsNullOrEmpty(Error.Code))
            {
                switch (Error.Code)
                {
                    case "rate_limit_exceeded": return "請稍後再嘗試。";
                    case "context_length_exceeded": return "抱歉，超出字數限制，請減少後再嘗試。";
                }
            }
            
            if (Choices == null || !Choices.Any())
                return string.Empty;

            return !string.IsNullOrEmpty(Choices.First().Text)
                ? Choices.First().Text
                : Choices.First().Message?.Content;
        }
    }
}

public class CompletionsChoiceDto
{
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("index")]
    public int Index { get; set; }
    
    [JsonProperty("delta")]
    public CompletionsDeltaDto Delta { get; set; }

    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; }
    
    [JsonProperty("logprobs")]
    public CompletionsLogProbsDto LogProbs { get; set; }
    
    [JsonProperty("message")]
    public CompletionsChoiceMessageDto Message { get; set; }
}

public class CompletionsChoiceMessageDto
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }
    
    [JsonProperty("function_call")]
    public CompletionsChoiceMessageFunctionCallDto FunctionCall { get; set; }
}

public class CompletionsChoiceMessageFunctionCallDto
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("arguments")]
    public string Arguments { get; set; }
}

public class CompletionsLogProbsDto
{
    [JsonProperty("tokens")]
    public List<string> Tokens { get; set; }

    [JsonProperty("token_logprobs")]
    public List<double> TokenLogProbs { get; set; }

    [JsonProperty("top_logprobs")]
    public IList<IDictionary<string, double>> TopLogProbs { get; set; }

    [JsonProperty("text_offset")]
    public List<int> TextOffsets { get; set; }
}

public class CompletionsUsageDto
{
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }
}

public class CompletionsErrorDto
{
    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("param")]
    public object Param { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }
}

public class CompletionsDeltaDto
{
    [JsonProperty("role")]
    public string Role { get; set; }
    
    [JsonProperty("content")]
    public string Content { get; set; }
}