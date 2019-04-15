using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Handlers
{
    public class CommandHandler
    {
        private static CommandService _commands;
        private static DiscordSocketClient _client;
        private static IServiceProvider _services;

        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(CommandHandler));

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;

            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync() => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        private static async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message) || message.Author.Id == _client.CurrentUser.Id ||
                message.Author.IsBot || message.Channel.GetType() == typeof(SocketDMChannel))
                return;

            var user = await UserService.FindUserAsync(message.Author.Id, message.Author.Username);

            if (await user.MessageRecievedAsync((uint)message.Content.Length, message.Author.Username))
            {
                await user.UpdateUserAsync();

                await message.Channel.SendFileAsync(user.DrawLevelUpImage(), "levelup.png");

                Logger.Information("Level up for {name}({uid}), new level -> {newlevel}", message.Author.Username, message.Author.Id, user.Level);
            }

            int argPos = 0;
            if (!message.HasStringPrefix(Config.Instance.BotPrefix, ref argPos))
                return;

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        private static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result.IsSuccess)
            {
                Logger.Information("Command {command} used by {name}({uid}).", context.Message.Content, context.User.Username, context.User.Id);
                return;
            }

            switch (result.Error)
            {
                case CommandError.UnknownCommand:
                    await context.Channel.SendMessageAsync("Command not found.");
                    Logger.Information("Non-existing command {command} used by {name}({uid}).", context.Message.Content, context.User.Username, context.User.Id);
                    break;

                case CommandError.UnmetPrecondition:
                    Logger.Warning("Command {command} is on cooldown for {name}({uid}).", context.Message, context.User.Username, context.User.Id);
                    await context.Channel.SendMessageAsync(result.ToString().Substring(19, result.ToString().Length - 19));
                    break;

                case CommandError.BadArgCount:
                case CommandError.ParseFailed:
                    Logger.Information("{name}({uid}) used a command wrong -> .", context.User.Username, context.User.Id, context.Message.Content);
                    await context.Channel.SendMessageAsync("Wrong usage.");
                    break;

                default:
                    await context.Channel.SendMessageAsync($"Something went wrong -> {result.ToString()}");
                    Logger.Error($"Something went wrong -> {result.ToString()}");
                    break;
            }
        }
    }
}
