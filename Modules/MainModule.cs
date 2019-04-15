using Discord;
using Discord.Commands;
using DiscordBot.Handlers;
using DiscordBot.Services;
using Serilog;
using Serilog.Core;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class MainModule : ModuleBase<SocketCommandContext>
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(MainModule));

        [Command("profile")]
        [Cooldown]
        public async Task ProfileCommand()
        {
            if (Context.User.GetAvatarUrl() == null)
            {
                await ReplyAsync("Please set up an avatar first.");
                Logger.Information("H!profile used by {name}({uid}) with empty avatar", Context.User.Username, Context.User.Id);
                return;
            }

            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            await Context.Channel.SendFileAsync(await user.DrawProfileImageAsync(await UserService.CalculateRankAsync(Context.User.Id), Context.User.GetAvatarUrl()), "profile.png");
        }

        [Command("profile")]
        [Cooldown]
        public async Task ProfileCommand(IUser mention)
        {
            if (mention.GetAvatarUrl() == null)
            {
                await ReplyAsync("User does not have an avatar.");
                Logger.Information("H!profile used by {name}({uid}) with empty avatar(mentioned)", Context.User.Username, Context.User.Id);
                return;
            }

            var user = await UserService.FindUserAsync(mention.Id, mention.Username);

            await Context.Channel.SendFileAsync(await user.DrawProfileImageAsync(await UserService.CalculateRankAsync(mention.Id), mention.GetAvatarUrl()), "profile.png");
        }

        [Command("rank")]
        [Cooldown]
        public async Task RankCommand()
        {
            if (Context.User.GetAvatarUrl() == null)
            {
                await ReplyAsync("Please set up an avatar first.");
                Logger.Information("H!rank used by {name}({uid}) with empty avatar", Context.User.Username, Context.User.Id);
                return;
            }

            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            await Context.Channel.SendFileAsync(await user.DrawRankImageAsync(await UserService.CalculateRankAsync(Context.User.Id), Context.User.GetAvatarUrl()), "rank.png");
        }

        [Command("rank")]
        [Cooldown]
        public async Task RankCommand(IUser mention)
        {
            if (mention.GetAvatarUrl() == null)
            {
                await ReplyAsync("User does not have an avatar.");
                Logger.Information("H!rank used by {name}({uid}) with empty avatar(mentioned)", Context.User.Username, Context.User.Id);
                return;
            }

            var user = await UserService.FindUserAsync(mention.Id, mention.Username);

            await Context.Channel.SendFileAsync(await user.DrawRankImageAsync(await UserService.CalculateRankAsync(mention.Id), mention.GetAvatarUrl()), "rank.png");
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

            await ReplyAsync("", false, embed.Build());
        }

        [Command("buy")]
        [Cooldown]
        public async Task BuyCommand(byte theme)
        {
            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            if (theme <= 0 || theme > 10)
            {
                await ReplyAsync("Theme not found.");
                Logger.Information("H!buy used by {name}({uid}) with unknown theme -> {theme}", Context.User.Username, Context.User.Id, theme);
                return;
            }

            if (user.Pen < 50000)
            {
                await ReplyAsync("Not enough pen.");
                Logger.Information("H!buy used by {name}({uid}) with insufficient pen -> {pen}", Context.User.Username, Context.User.Id, user.Pen);
                return;
            }

            var inventory = await InventoryService.FindInventory(Context.User.Id);

            var i = InventoryService.FindTheme(theme);

            if (inventory.CheckInventory(i))
            {
                await ReplyAsync("You already have that theme.");
                Logger.Information("H!buy used by {name}({uid}) with already acquired theme -> ", Context.User.Username, Context.User.Id, i.GetHashCode());
                return;
            }

            inventory.AddItem(i.ToString());
            await inventory.UpdateInventoryAsync();

            user.ProfileTheme = (byte)(i.GetHashCode());
            user.Pen -= 50000;
            await user.UpdateUserAsync();

            await ReplyAsync("Theme successfully bought!");
        }

        [Command("use")]
        [Cooldown]
        public async Task UseCommand(byte theme)
        {
            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            if (theme < 0 || theme > 10)
            {
                await ReplyAsync("Theme not found.");
                Logger.Information("H!use used by {name}({uid}) with unknown theme -> {theme}", Context.User.Username, Context.User.Id, theme);
                return;
            }

            var inventory = await InventoryService.FindInventory(Context.User.Id);

            var i = InventoryService.FindTheme(theme);

            if (!inventory.CheckInventory(i))
            {
                await ReplyAsync("You dont have that theme.");
                Logger.Information("H!use used by {name}({uid}) without the theme -> {theme}", Context.User.Username, Context.User.Id, i.GetHashCode());
                return;
            }

            user.ProfileTheme = (byte)(i.GetHashCode());
            await user.UpdateUserAsync();

            await ReplyAsync("Theme successfully equipped!");
        }

        [Command("daily")]
        [Cooldown(60, true)]
        public async Task DailyCommand()
        {
            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            if (user.LastDaily == null || UserService.CheckDaily(DateTime.Parse(user.LastDaily)))
            {
                uint penGain = UserService.CalculateDaily() * 1000;

                user.Pen += penGain;
                user.LastDaily = DateTime.Now.ToString();
                await user.UpdateUserAsync();

                user.DrawDailyImage(penGain);
                await Context.Channel.SendFileAsync(AppDomain.CurrentDomain.BaseDirectory + "resources\\dailytemp_" + Context.User.Id + ".png");
                return;
            }

            await ReplyAsync("You can claim your daily in " + (24 - (DateTime.Now - DateTime.Parse(user.LastDaily)).Hours).ToString() + "h");
            Logger.Information("H!daily used by {name}({uid}) while on cooldown, cooldown left -> {hours} h", Context.User.Username, Context.User.Id, (24 - (DateTime.Now - DateTime.Parse(user.LastDaily)).Hours).ToString());
        }

        [Command("top")]
        [Cooldown(60, true)]
        public async Task TopCommand()
        {
            var rankList = await UserService.GetTopListAsync();

            var embed = new EmbedBuilder();
            embed.WithTitle("Rank list");
            embed.WithColor(Color.Purple);

            uint count = 0;
            foreach (var player in rankList)
            {
                count++;

                embed.AddField("#" + count.ToString() + " " + player.Name, "Exp: " + player.Exp, true);

                if (count == 10)
                    break;
            }

            await ReplyAsync("", false, embed.Build());
        }

        [Command("gamble")]
        [Cooldown(30, true)]
        public async Task GambleCommand(uint amount)
        {
            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            if (amount > user.Pen)
            {
                await ReplyAsync("You don't have enough pen.");
                Logger.Information("H!gamble used by {name}({uid}) with insufficient pen -> {currentPen}({amount})", Context.User.Username, Context.User.Id, user.Pen, amount);
                return;
            }

            if (UserService.GambleIsWon() == true)
            {
                uint multiplier = UserService.GambleCalculateMultiplier();

                user.Pen += amount * (multiplier - 1);
                await user.UpdateUserAsync();
                await ReplyAsync($"Congratz, you have won {amount * (multiplier - 1)} pen!");
                Logger.Information("H!gamble won by {name}({uid}) -> {amount} pen", Context.User.Username, Context.User.Id, amount * (multiplier - 1));
                return;
            }

            user.Pen -= amount;
            await user.UpdateUserAsync();
            await ReplyAsync("Sadly, you have lost : ^(");
            Logger.Information("H!gamble lost by {name}({uid}) -> {amount} pen", Context.User.Username, Context.User.Id, amount);
        }

        [Command("shutdown")]
        [Cooldown]
        public async Task ShutdownCommand()
        {
            if (Context.User.Id != Config.Instance.OwnerId)
            {
                Logger.Warning("H!shutdown used by {name}({uid}) with no permission.", Context.User.Username, Context.User.Id);
                await ReplyAsync("No permission!");
                return;
            }
            Environment.Exit(0);
        }
    }
}
