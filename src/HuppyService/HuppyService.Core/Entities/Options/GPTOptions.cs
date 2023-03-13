namespace HuppyService.Core.Entities.Options
{
    public class GPTOptions
    {
        public string? BaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? Orgranization { get; set; }
        public int FreeMessageQuota { get; set; }
        public string? AiContextMessage { get; set; }
        public bool IsEnabled { get; set; }
    }
}
