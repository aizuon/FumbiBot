using DiscordBot.Helpers;
using Serilog;
using System;

namespace DiscordBot
{
    public static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
                .WriteTo.Async(w => w.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}"), bufferSize: 1000, blockWhenFull: true)
                .CreateLogger();

            Database.Initialize();
        }

        private static void Main()
        {
            Bot.Start(Config.Instance.BotToken);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Bot.Stop();

            ImageCache.Dispose();

            Log.CloseAndFlush();
        }
    }
}
