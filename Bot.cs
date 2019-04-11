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

        private static readonly ILogger Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger().ForContext(Constants.SourceContextPropertyName, nameof(Discord));

        public static void Start(string token)
        {
            s_token = token;

            Database.Open();

            MainAsync().GetAwaiter().GetResult();

            Database.Close();
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
            Logger.Debug(log.ToString());

            return Task.CompletedTask;
        }

        private static Task ReadyAsync()
        {
            Logger.Debug($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        private static ServiceProvider ConfigureServices() => new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
    }
}
