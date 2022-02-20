using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
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

        [JsonIgnore]
        private readonly static string FILE_NAME = AppContext.BaseDirectory + "appsettings.json";

        [JsonIgnore]
        public static bool IsCreated
            => File.Exists(FILE_NAME);

        [JsonIgnore]
        public static string SavePath
            => Path.Combine(AppContext.BaseDirectory, @"Save/save.sqlite");

        public static AppSettings Load()
        {
            var readBytes = File.ReadAllBytes(FILE_NAME);
            var config = JsonSerializer.Deserialize<AppSettings>(readBytes);
            return config!;
        }

        public static AppSettings Create()
        {
            if (IsCreated)
                return Load();

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "save")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "save"));

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
                    Orgranization = ""
                }
            };

            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            File.WriteAllBytes(FILE_NAME, JsonSerializer.SerializeToUtf8Bytes(config, options));

            return config;
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
    }
}