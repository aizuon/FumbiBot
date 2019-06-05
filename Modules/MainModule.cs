using Discord;
using Discord.Commands;
using DiscordBot.Handlers;
using DiscordBot.Helpers;
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
        [Summary("Displays user's own profile.")]
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
        [Summary("Displays another user's profile.")]
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

        [Command("avatar")]
        [Summary("Displays user's own avatar.")]
        [Cooldown]
        public async Task AvatarCommand()
        {
            if (Context.User.GetAvatarUrl() == null)
            {
                await ReplyAsync("Please set up an avatar first.");
                Logger.Information("H!avatar used by {name}({uid}) with empty avatar", Context.User.Username, Context.User.Id);
                return;
            }

            await Context.Channel.SendFileAsync(await GraphicsHelper.GetAvatarStreamAsync(Context.User.GetAvatarUrl(ImageFormat.Auto, 512)), "avatar.png");
        }

        [Command("avatar")]
        [Summary("Displays another user's avatar.")]
        [Cooldown]
        public async Task AvatarCommand(IUser mention)
        {
            if (mention.GetAvatarUrl() == null)
            {
                await ReplyAsync("User does not have an avatar.");
                Logger.Information("H!avatar used by {name}({uid}) with empty avatar(mentioned)", Context.User.Username, Context.User.Id);
                return;
            }

            await Context.Channel.SendFileAsync(await GraphicsHelper.GetAvatarStreamAsync(mention.GetAvatarUrl(ImageFormat.Auto, 512)), "avatar.png");
        }

        [Command("rank")]
        [Summary("Displays user's own rank.")]
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
        [Summary("Displays another user's rank.")]
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
        [Summary("Displays items in the shop.")]
        [Cooldown]
        public async Task ShopCommand()
        {
            var embed = new EmbedBuilder();
            embed.WithDescription("Write H!buy + themes id to buy, each theme is 250k pen");
            embed.WithTitle("Profile theme shop");
            foreach (var theme in Enum.GetValues(typeof(Shop.ProfileTheme)).Cast<Shop.ProfileTheme>().Skip(1).ToArray())
                embed.AddField(theme.ToString(), theme.GetHashCode(), true);

            await ReplyAsync("", false, embed.Build());
        }

        [Command("buy")]
        [Summary("Buys items from the shop.")]
        [Cooldown]
        public async Task BuyCommand(byte theme)
        {
            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            if (theme <= 0 || theme > 8)
            {
                await ReplyAsync("Theme not found.");
                Logger.Information("H!buy used by {name}({uid}) with unknown theme -> {theme}", Context.User.Username, Context.User.Id, theme);
                return;
            }

            if (user.Pen < 250000)
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
            user.Pen -= 250000;
            await user.UpdateUserAsync();

            await ReplyAsync("Theme successfully bought!");
        }

        [Command("use")]
        [Summary("Equips items from user's inventory.")]
        [Cooldown]
        public async Task UseCommand(byte theme)
        {
            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            if (theme < 0 || theme > 8)
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
        [Summary("Claims daily pen reward.")]
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

                await Context.Channel.SendFileAsync(user.DrawDailyImage(penGain), "daily.png");
                return;
            }

            await ReplyAsync("You can claim your daily in " + (24 - (DateTime.Now - DateTime.Parse(user.LastDaily)).Hours).ToString() + "h");
            Logger.Information("H!daily used by {name}({uid}) while on cooldown, cooldown left -> {hours} h", Context.User.Username, Context.User.Id, (24 - (DateTime.Now - DateTime.Parse(user.LastDaily)).Hours).ToString());
        }

        [Command("dailyexp")]
        [Summary("Displays daily exp limit status.")]
        [Cooldown(60, true)]
        public async Task DailyExpCommand()
        {
            var user = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            var lastdaily = DateTime.Parse(user.DailyExp.Remove(0, 8));
            uint totalexp = uint.Parse(user.DailyExp.Remove(5));

            if (totalexp >= 75000 && (lastdaily - DateTime.Now).Days < 1)
                await ReplyAsync("You have capped your daily exp limit and it will reset in " + (24 - (DateTime.Now - lastdaily).Hours).ToString() + "h");

            if (totalexp < 75000)
                await ReplyAsync(75000 - totalexp + " exp remaining for today");
        }

        [Command("top")]
        [Summary("Displays top rank list.")]
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
        [Summary("Used to play gamble with pen.")]
        [Cooldown(30, true)]
        public async Task GambleCommand(ulong amount)
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

        [Command("givepen")]
        [Summary("Gives pen to a user, admin only.")]
        [Cooldown]
        public async Task PenCommand(IUser mention, ulong amount)
        {
            if (Context.User.Id != Config.Instance.OwnerId)
            {
                Logger.Warning("H!givepen used by {name}({uid}) with no permission.", Context.User.Username, Context.User.Id);
                await ReplyAsync("No permission!");
                return;
            }

            var user = await UserService.FindUserAsync(mention.Id, mention.Username);

            user.Pen += amount;
            await user.UpdateUserAsync();
        }

        [Command("transfer")]
        [Summary("Transfers pen from own balance to another user.")]
        [Cooldown]
        public async Task TransferCommand(IUser mention, ulong amount)
        {
            var transferrer = await UserService.FindUserAsync(Context.User.Id, Context.User.Username);

            if (transferrer.Pen < amount)
            {
                await ReplyAsync("You don't have enough pen.");
                Logger.Information("H!transfer used by {name}({uid}) with insufficient pen -> {currentPen}({amount})", Context.User.Username, Context.User.Id, transferrer.Pen, amount);
                return;
            }

            transferrer.Pen -= amount;
            await transferrer.UpdateUserAsync();

            var transferee = await UserService.FindUserAsync(mention.Id, mention.Username);

            transferee.Pen += amount;
            await transferee.UpdateUserAsync();
        }

        [Command("giveexp")]
        [Summary("Gives exp to a user, admin only.")]
        [Cooldown]
        public async Task ExpCommand(IUser mention, uint amount)
        {
            if (Context.User.Id != Config.Instance.OwnerId)
            {
                Logger.Warning("H!giveexp used by {name}({uid}) with no permission.", Context.User.Username, Context.User.Id);
                await ReplyAsync("No permission!");
                return;
            }

            var user = await UserService.FindUserAsync(mention.Id, mention.Username);

            user.Exp += amount;
            user.Level = UserService.CalculateLevel(user.Exp);
            await user.UpdateUserAsync();
        }

        [Command("help")]
        [Summary("Displays this message.")]
        [Cooldown]
        public async Task HelpCommand()
        {
            var commands = CommandHandler._commands.Commands.ToList();
            var embedBuilder = new EmbedBuilder();

            foreach (var command in commands)
                embedBuilder.AddField(command.Name, command.Summary ?? "No description available");

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }

        [Command("shutdown")]
        [Summary("Terminates the bot, owner only.")]
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
