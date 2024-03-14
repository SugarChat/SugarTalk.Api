using Newtonsoft.Json;
using System.Collections.Generic;

namespace SugarTalk.Messages.Dto.OpenAi;

public abstract class CompletionsRequestBaseDto
{
    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonProperty("max_tokens", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxTokens { get; set; }
    
    [JsonProperty("temperature")]
    public double Temperature { get; set; }

    [JsonProperty("frequency_penalty")] 
    public double FrequencyPenalty { get; set; } = 0.5;
}

public class ChatCompletionsRequestDto : CompletionsRequestBaseDto
{
    [JsonProperty("messages")]
    public List<CompletionsRequestMessageDto> Messages { get; set; } = new();

    [JsonProperty("functions", NullValueHandling = NullValueHandling.Ignore)]
    public List<CompletionsRequestFunctionDto> Functions { get; set; }

    [JsonProperty("function_call", NullValueHandling = NullValueHandling.Ignore)]
    public object FunctionCall { get; set; }
    
    [JsonProperty("stream", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Stream { get; set; }
    
    [JsonProperty("response_format", NullValueHandling = NullValueHandling.Ignore)]
    public CompletionResponseFormatDto ResponseFormat { get; set; }
}

public class CompletionsRequestMessageDto
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }
}

public class CompletionsRequestFunctionDto
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }
    
    [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
    public CompletionsRequestFunctionParametersDto Parameters { get; set; }
}

public class CompletionsRequestFunctionParametersDto
{
    [JsonProperty("type")]
    public string Type { get; set; } = "object";

    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Required { get; set; }
    
    [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, CompletionsRequestFunctionParameterPropertyValue> Properties { get; set; }
}

public class CompletionsRequestFunctionParameterPropertyValue
{
    [JsonProperty("type")]
    public string Type { get; set; } = "string";

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Enum { get; set; }
}

public class CompletionsRequestFunctionCallDto
{
    [JsonProperty("name")] 
    public string Name { get; set; } = "none";
    
    [JsonProperty("arguments")]
    public string Arguments { get; set; }
}

public class CompletionResponseFormatDto
{
    [JsonProperty("type")]
    public string Type { get; set; }
}
