using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System.Threading.Tasks;

namespace DiscordBot
{
    public static class Bot
    {
        private static DiscordSocketClient _client;
        private static string s_token;
        private static ServiceProvider _services;

        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Bot));

        public static void Start(string token)
        {
            s_token = token;

            MainAsync().GetAwaiter().GetResult();

        }

        private static async Task MainAsync()
        {
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _services.GetRequiredService<CommandService>().Log += LogAsync;

            await _client.LoginAsync(TokenType.Bot, s_token);
            await _client.StartAsync();

            await _services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(-1);
        }

        private static Task LogAsync(LogMessage log)
        {
            if (log.Message.StartsWith("Executed"))
                return Task.CompletedTask;

            switch (log.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Logger.Error(log.Message);
                    break;

                case LogSeverity.Warning:
                    Logger.Warning(log.Message);
                    break;

                case LogSeverity.Debug:
                    Logger.Debug(log.Message);
                    break;

                default:
                    Logger.Information(log.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        private static Task ReadyAsync()
        {
            Logger.Information($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        private static ServiceProvider ConfigureServices() => new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Verbose, ThrowOnError = false, IgnoreExtraArgs = false }))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
    }
}
