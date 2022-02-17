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
        public string HomeGuild { get; set; }
        public string? ConnectionString { get; set; }
        public Logger Logger { get; set; }

        [JsonIgnore]
        private static string FILE_NAME = AppContext.BaseDirectory + "appsettings.json";

        [JsonIgnore]
        public static bool IsCreated
            => File.Exists(FILE_NAME);

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

            var config = new AppSettings()
            {
                BotToken = "",
                HomeGuild = "",
                ConnectionString = "",
                Logger = new Logger()
                {
                    LogLevel = "information",
                    TimeInterval = "hour"
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
        public string LogLevel { get; set; }
        public string TimeInterval { get; set; }
    }
}