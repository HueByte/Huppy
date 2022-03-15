using System.Text.Json;
using System.Text.Json.Serialization;

namespace Huppy.Core.Entities
{
    public class AppSettings
    {
        public string? BotToken { get; set; }
        public string? HomeGuilds { get; set; }
        public string? ConnectionString { get; set; }
        public Logger? Logger { get; set; }
        public GPT? GPT { get; set; }
        public UrbanApi? UrbanApi { get; set; }
        public NewsAPI? NewsAPI { get; set; }

        [JsonIgnore]
        private readonly static string FILE_NAME = AppContext.BaseDirectory + "appsettings.json";

        [JsonIgnore]
        public static bool IsCreated
            => File.Exists(FILE_NAME);

        [JsonIgnore]
        public static string SavePath
            => Path.Combine(AppContext.BaseDirectory, "save", "save.sqlite");

        public static AppSettings Load()
        {
            var readBytes = File.ReadAllBytes(FILE_NAME);
            var config = JsonSerializer.Deserialize<AppSettings>(readBytes);

            CreateFolders();

            return config!;
        }

        public static AppSettings Create()
        {
            if (IsCreated)
                return Load();

            CreateFolders();

            var config = new AppSettings()
            {
                BotToken = "",
                HomeGuilds = "",
                ConnectionString = $"Data Source={SavePath}",
                Logger = new()
                {
                    LogLevel = "information",
                    TimeInterval = "hour"
                },
                GPT = new()
                {
                    BaseUrl = "",
                    ApiKey = "",
                    Orgranization = "",
                    FreeMessageQuota = 0,
                    IsEnabled = false,
                },
                UrbanApi = new()
                {
                    BaseUrl = "https://mashape-community-urban-dictionary.p.rapidapi.com/define",
                    Host = "mashape-community-urban-dictionary.p.rapidapi.com",
                    Key = ""
                },
                NewsAPI = new()
                {
                    BaseUrl = "",
                    ApiKey = ""
                }
            };

            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            File.WriteAllBytes(FILE_NAME, JsonSerializer.SerializeToUtf8Bytes(config, options));

            return config;
        }

        private static void CreateFolders()
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "save")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "save"));
        }
    }

    public class Logger
    {
        public string? LogLevel { get; set; }
        public string? TimeInterval { get; set; }
    }

    public class GPT
    {
        public string? BaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? Orgranization { get; set; }
        public int FreeMessageQuota { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class UrbanApi
    {
        public string? BaseUrl { get; set; }
        public string? Host { get; set; }
        public string? Key { get; set; }
    }

    public class NewsAPI
    {
        public string? BaseUrl { get; set; }
        public string? ApiKey { get; set; }
    }
}