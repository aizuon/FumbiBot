using DiscordBot.Helpers;
using DiscordBot.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace DiscordBot
{
    [Table("users")]
    public class User
    {
        [Key]
        public ulong Uid { get; set; }

        public string Name { get; set; }

        public byte Level { get; set; }

        public uint Exp { get; set; }

        public uint Pen { get; set; }

        public byte ProfileTheme { get; set; }

        public string LastDaily { get; set; }

        public void DrawLevelUpImage() => GraphicsHelper.DrawLevelUpImage(Level, Name, Uid, ProfileTheme);

        public void DrawProfileImage(uint rank) => GraphicsHelper.DrawProfileImage(Level, Name, Uid, Exp, Pen, rank, ProfileTheme, UserService.CalculateExpBar(Level, Exp));

        public void DrawRankImage(uint rank) => GraphicsHelper.DrawRankImage(Level, Name, Uid, rank, ProfileTheme, UserService.CalculateExpBar(Level, Exp));

        public void DrawDailyImage(uint penGain) => GraphicsHelper.DrawDailyImage(penGain, Uid);

        public async Task<bool> OnMessageRecieved(uint length, string name) => await UserService.OnMessageRecieved(length, name, this);

        public async Task<bool> UpdateUserAsync() => await UserService.UpdateUserAsync(this);
    }
}
