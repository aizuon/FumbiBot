﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DiscordBot.Handlers
{
    public class Cooldown : PreconditionAttribute
    {
        private TimeSpan CooldownLength;
        private bool AdminsAreLimited;
        private static readonly ConcurrentDictionary<CooldownInfo, DateTime> _cooldowns = new ConcurrentDictionary<CooldownInfo, DateTime>();

        private static readonly ILogger Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger().ForContext(Constants.SourceContextPropertyName, nameof(Cooldown));

        public Cooldown()
        {
            CooldownLength = TimeSpan.FromSeconds(30);
            AdminsAreLimited = false;
        }

        public Cooldown(uint seconds, bool areAdminsLimited)
        {
            CooldownLength = TimeSpan.FromSeconds(seconds);
            AdminsAreLimited = areAdminsLimited;
        }

        public struct CooldownInfo
        {
            public ulong UserId { get; }
            public int CommandHashCode { get; }

            public CooldownInfo(ulong userId, int commandHashCode)
            {
                UserId = userId;
                CommandHashCode = commandHashCode;
            }
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as SocketUser;

            if (user.Id == Config.Instance.OwnerId)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            if (!AdminsAreLimited && ((IGuildUser)(user)).GuildPermissions.Administrator)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            var key = new CooldownInfo(user.Id, command.GetHashCode());
            if (_cooldowns.TryGetValue(key, out var endsAt))
            {
                var difference = endsAt.Subtract(DateTime.UtcNow);
                if (difference.Ticks > 0)
                {
                    Logger.Warning("Command {command} is on cooldown for {name}({uid}).", command.Name, user.Username, user.Id);
                    return await Task.FromResult(PreconditionResult.FromError($"You can use this command in {difference.ToString(@"mm\:ss")}"));
                }
                var time = DateTime.UtcNow.Add(CooldownLength);
                _cooldowns.TryUpdate(key, time, endsAt);
            }
            else
            {
                _cooldowns.TryAdd(key, DateTime.UtcNow.Add(CooldownLength));
            }

            return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
