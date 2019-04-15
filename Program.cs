using DiscordBot.Helpers;
using Serilog;

namespace DiscordBot
{
    public static class Program
    {
        static void Main()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
                .WriteTo.Async(w => w.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}"), bufferSize: 1000, blockWhenFull: true)
                .CreateLogger();

            Database.Open();

            Bot.Start(Config.Instance.BotToken);

            Database.Close();

            ImageCache.Dispose();

            Log.CloseAndFlush();
        }
    }
}
