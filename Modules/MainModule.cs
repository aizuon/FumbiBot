using Discord;
using Discord.Commands;
using DiscordBot.Handlers;
using DiscordBot.Services;
using Serilog;
using Serilog.Core;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class MainModule : ModuleBase<SocketCommandContext>
    {
        private static readonly ILogger Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger().ForContext(Constants.SourceContextPropertyName, nameof(MainModule));

        [Command("profile")]
        [Cooldown]
        public async Task ProfileCommand()
        {
            var user = await UserService.FindUser(Context.User.Id, Context.User.Username);

            using (var client = new WebClient())
                client.DownloadFile(new Uri(Context.User.GetAvatarUrl()),
                    AppDomain.CurrentDomain.BaseDirectory + "resources\\avatartemp_ " + Context.User.Id + ".png");

            user.DrawProfileImage(await UserService.CalculateRankAsync(Context.User.Id));
            await Context.Channel.SendFileAsync(AppDomain.CurrentDomain.BaseDirectory + "resources\\profiletemp_ " + Context.User.Id + ".png");
        }

        [Command("profile")]
        [Cooldown]
        public async Task ProfileCommand(IUser mention)
        {
            var user = await UserService.FindUser(mention.Id, mention.Username);

            using (var client = new WebClient())
                client.DownloadFile(new Uri(mention.GetAvatarUrl()),
                    AppDomain.CurrentDomain.BaseDirectory + "resources\\avatartemp_ " + mention.Id + ".png");

            user.DrawProfileImage(await UserService.CalculateRankAsync(mention.Id));
            await Context.Channel.SendFileAsync(AppDomain.CurrentDomain.BaseDirectory + "resources\\profiletemp_ " + mention.Id + ".png");
        }

        [Command("shop")]
        [Cooldown]
        public async Task ShopCommand()
        {
            var embed = new EmbedBuilder();
            embed.WithDescription("Write H!buy + themes id to buy, each theme is 50k pen");
            embed.WithTitle("Profile theme shop");
            foreach (var theme in Enum.GetValues(typeof(Shop.ProfileTheme)).Cast<Shop.ProfileTheme>().Skip(1).ToArray())
                embed.AddField(theme.ToString(), theme.GetHashCode(), true);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("buy")]
        [Cooldown]
        public async Task BuyCommand(byte theme)
        {
            var user = await UserService.FindUser(Context.User.Id, Context.User.Username);

            if (theme <= 0 || theme > 8)
            {
                await Context.Channel.SendMessageAsync("Theme not found.");
                Logger.Information("H!buy used by {name}({uid}) with unknown theme -> {theme}", Context.User.Username, Context.User.Id, theme);
                return;
            }

            if (user.Pen < 50000)
            {
                await Context.Channel.SendMessageAsync("Not enough pen.");
                Logger.Information("H!buy used by {name}({uid}) with insufficient pen -> {pen}", Context.User.Username, Context.User.Id, user.Pen);
                return;
            }

            var inventory = await InventoryService.FindInventory(Context.User.Id);

            var i = InventoryService.FindTheme(theme);

            inventory.AddItem(i.ToString());
            inventory.UpdateInventoryAsync();

            user.ProfileTheme = (byte)(i.GetHashCode());
            user.Pen -= 50000;
            user.UpdateUserAsync();

            await Context.Channel.SendMessageAsync("Theme successfully bought!");
            Logger.Information("H!buy used by {name}({uid}), theme -> {theme}", Context.User.Username, Context.User.Id, i.GetHashCode());
        }

        [Command("use")]
        [Cooldown]
        public async Task UseCommand(byte theme)
        {
            var user = await UserService.FindUser(Context.User.Id, Context.User.Username);

            if (theme < 0 || theme > 8)
            {
                await Context.Channel.SendMessageAsync("Theme not found.");
                Logger.Information("H!use used by {name}({uid}) with unknown theme -> {theme}", Context.User.Username, Context.User.Id, theme);
                return;
            }

            var inventory = await InventoryService.FindInventory(Context.User.Id);

            var i = InventoryService.FindTheme(theme);

            if (!inventory.CheckInventory(i))
            {
                await Context.Channel.SendMessageAsync("You dont have that theme.");
                Logger.Information("H!use used by {name}({uid}) without the theme -> {theme}", Context.User.Username, Context.User.Id, i.GetHashCode());
                return;
            }

            user.ProfileTheme = (byte)(i.GetHashCode());
            user.UpdateUserAsync();

            await Context.Channel.SendMessageAsync("Theme successfully equipped!");
            Logger.Information("H!use used by {name}({uid}), theme -> {theme}", Context.User.Username, Context.User.Id, i.GetHashCode());
        }

        [Command("shutdown")]
        [Cooldown]
        public async Task ShutdownCommand()
        {
            if (Context.User.Id != Config.Instance.OwnerId)
            {
                await Context.Channel.SendMessageAsync("No permission!");
                return;
            }
            Environment.Exit(0);
        }
    }
}
