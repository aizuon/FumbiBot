using Dapper.FastCrud;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Handlers;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using System.Linq;
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

            StartAsync().GetAwaiter().GetResult();
        }

        private static async Task StartAsync()
        {
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();

            _client.Log += LogAsync;
            _services.GetRequiredService<CommandService>().Log += LogAsync;

            await _client.LoginAsync(TokenType.Bot, s_token);
            await _client.StartAsync();

            await _services.GetRequiredService<CommandHandler>().InitializeAsync();

            _client.GuildAvailable += GuildAvaliableAsync;

            await Task.Delay(-1);
        }

        private static async Task GuildAvaliableAsync(SocketGuild g)
        {
            if (g.GetUser(_client.CurrentUser.Id).Roles.Where(r => r.Permissions.ManageRoles == true).ToList().Count == 0)
                return;

            var rookie = g.Roles.FirstOrDefault(x => x.Name.ToString() == "Rookie");
            var ama = g.Roles.FirstOrDefault(x => x.Name.ToString() == "Amateur");
            var semipro = g.Roles.FirstOrDefault(x => x.Name.ToString() == "Semi-Pro");
            var pro = g.Roles.FirstOrDefault(x => x.Name.ToString() == "Pro");
            var s4 = g.Roles.FirstOrDefault(x => x.Name.ToString() == "S4");

            if (rookie == null && ama == null && semipro == null && pro == null && s4 == null)
                return;

            Logger.Information("Checking roles for {guildName}...", g.Name);

            List<User> dbusers;

            using (var db = Database.Open())
                dbusers = (await db.FindAsync<User>()).ToList();

            var users = g.Users.ToList();

            foreach (var u in users)
            {
                if (u.IsBot)
                    continue;

                var dbu = dbusers.Find(dbuser => dbuser.Uid == u.Id);
                if (dbu == null)
                    continue;

                Logger.Information("Proccessing user {username}({userid})...", dbu.Name, dbu.Uid);

                if (u.Username != dbu.Name)
                {
                    UserService.UpdateUsername(u.Username, dbu);
                    await dbu.UpdateUserAsync();
                }

                if (dbu.Level < 20)
                {
                    if (!u.Roles.Contains(rookie))
                    {
                        await u.AddRoleAsync(rookie);
                    }

                    if (u.Roles.Contains(ama) || u.Roles.Contains(semipro) || u.Roles.Contains(pro) || u.Roles.Contains(s4))
                        await u.RemoveRolesAsync(new List<SocketRole> { ama, semipro, pro, s4 });
                }

                else if (20 <= dbu.Level && dbu.Level < 40)
                {
                    if (!u.Roles.Contains(ama))
                    {
                        await u.AddRoleAsync(ama);
                    }

                    if (u.Roles.Contains(rookie) || u.Roles.Contains(semipro) || u.Roles.Contains(pro) || u.Roles.Contains(s4))
                        await u.RemoveRolesAsync(new List<SocketRole> { rookie, semipro, pro, s4 });
                }

                else if (40 <= dbu.Level && dbu.Level < 60)
                {
                    if (!u.Roles.Contains(semipro))
                    {
                        await u.AddRoleAsync(semipro);
                    }

                    if (u.Roles.Contains(rookie) || u.Roles.Contains(ama) || u.Roles.Contains(pro) || u.Roles.Contains(s4))
                        await u.RemoveRolesAsync(new List<SocketRole> { rookie, ama, pro, s4 });
                }

                else if (60 <= dbu.Level && dbu.Level < 80)
                {
                    if (!u.Roles.Contains(pro))
                    {
                        await u.AddRoleAsync(pro);
                    }

                    if (u.Roles.Contains(rookie) || u.Roles.Contains(ama) || u.Roles.Contains(semipro) || u.Roles.Contains(s4))
                        await u.RemoveRolesAsync(new List<SocketRole> { rookie, ama, semipro, s4 });
                }

                else if (dbu.Level == 80)
                {
                    if (!u.Roles.Contains(s4))
                    {
                        await u.AddRoleAsync(s4);
                    }

                    if (u.Roles.Contains(rookie) || u.Roles.Contains(ama) || u.Roles.Contains(semipro) || u.Roles.Contains(pro))
                        await u.RemoveRolesAsync(new List<SocketRole> { rookie, ama, semipro, pro });
                }
            }

            Logger.Information("Done!");
        }

        private static async Task LogAsync(LogMessage log)
        {
            if (log.Message.StartsWith("Executed"))
                return;

            if (log.Exception is CommandException cmdException)
            {
                await cmdException.Context.Channel.SendMessageAsync("Something went terribly wrong -> " + cmdException.Message);
                Logger.Error("{command} failed to execute by {user}({uid}).", cmdException.Context.Message, cmdException.Context.User.Username, cmdException.Context.User.Id);
            }

            else
            {
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
            }
        }

        public static void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        private static async Task StopAsync()
        {
            await _client.LogoutAsync();
            await _client.StopAsync();

            _client.Dispose();
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Info, MessageCacheSize = 0 }))
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Verbose, ThrowOnError = false, IgnoreExtraArgs = false }))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }
    }
}
