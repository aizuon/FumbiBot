using DiscordBot.Helpers;
using DiscordBot.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
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

        public string DailyExp { get; set; }

        public MemoryStream DrawLevelUpImage() => GraphicsHelper.DrawLevelUpImage(Level, Name, ProfileTheme);

        public async Task<MemoryStream> DrawProfileImageAsync(uint rank, string avatarUrl) => await GraphicsHelper.DrawProfileImageAsync(Level, Name, Exp, Pen, rank, ProfileTheme, UserService.CalculateExpBar(Level, Exp), avatarUrl);

        public async Task<MemoryStream> DrawRankImageAsync(uint rank, string avatarUrl) => await GraphicsHelper.DrawRankImageAsync(Level, Name, rank, ProfileTheme, UserService.CalculateExpBar(Level, Exp), avatarUrl);

        public MemoryStream DrawDailyImage(uint penGain) => GraphicsHelper.DrawDailyImage(penGain);

        public async Task<bool> MessageRecievedAsync(uint length, string name) => await UserService.MessageRecievedAsync(length, name, this);

        public async Task<bool> UpdateUserAsync() => await UserService.UpdateUserAsync(this);
    }
}
