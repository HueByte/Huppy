using System.Text.Json.Serialization;

namespace Huppy.Core.Dto
{
    public class List
    {
        [JsonPropertyName("definition")]
        public string? Definition { get; set; }

        [JsonPropertyName("permalink")]
        public string? Permalink { get; set; }

        [JsonPropertyName("thumbs_up")]
        public int ThumbsUp { get; set; }

        [JsonPropertyName("sound_urls")]
        public List<string?>? SoundUrls { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("word")]
        public string? Word { get; set; }

        [JsonPropertyName("defid")]
        public int Defid { get; set; }

        [JsonPropertyName("current_vote")]
        public string? CurrentVote { get; set; }

        [JsonPropertyName("written_on")]
        public DateTime WrittenOn { get; set; }

        [JsonPropertyName("example")]
        public string? Example { get; set; }

        [JsonPropertyName("thumbs_down")]
        public int ThumbsDown { get; set; }
    }

    public class UrbanResponse
    {
        [JsonPropertyName("list")]
        public List<List>? List { get; set; }
    }
}