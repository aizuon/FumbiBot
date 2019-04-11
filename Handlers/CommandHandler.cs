﻿using Discord;
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
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;

        private static readonly ILogger Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger().ForContext(Constants.SourceContextPropertyName, nameof(CommandHandler));

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;

            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync() => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message) || message.Author.Id == _client.CurrentUser.Id ||
                message.Author.IsBot || message.Channel.GetType() == typeof(SocketDMChannel))
                return;

            var user = await UserService.FindUser(message.Author.Id, message.Author.Username);

            if (user.HasLeveledUp((uint)message.Content.Length))
            {
                user.UpdateUserAsync();

                user.DrawLevelUpImage();
                await message.Channel.SendFileAsync(AppDomain.CurrentDomain.BaseDirectory + "resources\\leveltemp_ " + message.Author.Id + ".png");

                Logger.Information("Level up for {name}({uid}), new level -> {newlevel}", message.Author.Username, message.Author.Id, user.Level);
            }

            int argPos = 0;
            if (!message.HasStringPrefix(Config.Instance.BotPrefix, ref argPos))
                return;

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
            {
                await context.Channel.SendMessageAsync("Command not found");
                return;
            }

            if (result.IsSuccess)
            {
                Logger.Information("{command} used by {name}({uid}).", context.Message.Content, context.User.Username, context.User.Id);
                return;
            }

            await context.Channel.SendMessageAsync($"Something went wrong -> error: {result}");
        }
    }
}
