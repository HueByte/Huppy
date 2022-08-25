using System.Text.Json.Serialization;

namespace Huppy.Core.Dto
{
    public class GPTDto
    {
        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("top_p")]
        public int TopP { get; set; }

        [JsonPropertyName("n")]
        public int N { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("logprobs")]
        public object? Logprobs { get; set; }

        [JsonPropertyName("stop")]
        public string[]? Stop { get; set; }

        [JsonPropertyName("frequency_penalty")]
        public double FrequencyPenalty { get; set; }

        [JsonPropertyName("presence_penalty")]
        public double PresencePenalty { get; set; }
    }
}