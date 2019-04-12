using DiscordBot.Helpers;
using DiscordBot.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        private enum Levels : uint
        {
            Level0 = 0,
            Level1 = 300,
            Level2 = 600,
            Level3 = 900,
            Level4 = 1200,
            Level5 = 1500,
            Level6 = 2200,
            Level7 = 2800,
            Level8 = 3500,
            Level9 = 4200,
            Level10 = 4900,
            Level11 = 6100,
            Level12 = 7300,
            Level13 = 8500,
            Level14 = 9700,
            Level15 = 10900,
            Level16 = 12600,
            Level17 = 14300,
            Level18 = 16000,
            Level19 = 17700,
            Level20 = 20000,
            Level21 = 22300,
            Level22 = 24600,
            Level23 = 26900,
            Level24 = 29200,
            Level25 = 33500,
            Level26 = 37800,
            Level27 = 42100,
            Level28 = 46400,
            Level29 = 50700,
            Level30 = 60500,
            Level31 = 70300,
            Level32 = 80100,
            Level33 = 89900,
            Level34 = 99700,
            Level35 = 126500,
            Level36 = 153300,
            Level37 = 180100,
            Level38 = 206900,
            Level39 = 233700,
            Level40 = 264500,
            Level41 = 295300,
            Level42 = 326100,
            Level43 = 356900,
            Level44 = 387700,
            Level45 = 428500,
            Level46 = 469300,
            Level47 = 510100,
            Level48 = 550900,
            Level49 = 591700,
            Level50 = 658500,
            Level51 = 725300,
            Level52 = 792100,
            Level53 = 858900,
            Level54 = 925700,
            Level55 = 1064500,
            Level56 = 1203300,
            Level57 = 1342100,
            Level58 = 1480900,
            Level59 = 1619700,
            Level60 = 1762500,
            Level61 = 1905300,
            Level62 = 2048100,
            Level63 = 2190900,
            Level64 = 2333700,
            Level65 = 2491500,
            Level66 = 2649300,
            Level67 = 2807100,
            Level68 = 2964900,
            Level69 = 3122700,
            Level70 = 3314500,
            Level71 = 3506300,
            Level72 = 3698100,
            Level73 = 3889900,
            Level74 = 4081700,
            Level75 = 4345500,
            Level76 = 4609300,
            Level77 = 4873100,
            Level78 = 5136900,
            Level79 = 5400700,
            Level80 = 5664500
        }

        public byte CalculateLevel()
        {
            int level = -1;

            foreach (uint i in Enum.GetValues(typeof(Levels)).Cast<Levels>())
            {
                if (Exp < i)
                    break;

                level++;
            }

            return (byte)level;
        }

        public uint CalculatePenGain()
        {
            if (Level < 4)
                return 2000;
            else if (Level >= 5 && Level < 10)
                return 3000;
            else if (Level >= 10 && Level < 20)
                return 4000;
            else if (Level >= 20 && Level < 27)
                return 7000;
            else if (Level >= 27 && Level < 35)
                return 10000;
            else if (Level >= 35 && Level < 42)
                return 13000;
            else if (Level >= 42 && Level < 47)
                return 15000;
            else if (Level >= 47 && Level < 55)
                return 17000;
            else if (Level >= 55 && Level < 62)
                return 20000;
            else if (Level >= 62 && Level < 67)
                return 24000;
            else if (Level >= 67 && Level < 73)
                return 27000;
            else if (Level >= 73 && Level < 80)
                return 30000;
            else
                return 100000;
        }

        public struct ExpBar
        {
            public float Percentage { get; set; }
            public uint CurrentExp { get; set; }
            public uint NextLevelExp { get; set; }
        }

        public ExpBar CalculateExpBar()
        {
            var expList = Enum.GetValues(typeof(Levels)).Cast<Levels>().ToList();
            float percentage = (((Exp - (float)expList[Level])) / ((float)expList[Level + 1] - (float)expList[Level]));

            var expBar = new ExpBar
            {
                Percentage = percentage,
                CurrentExp = Exp - (uint)expList[Level],
                NextLevelExp = (uint)expList[Level + 1] - (uint)expList[Level]
            };

            return expBar;
        }

        public void DrawLevelUpImage() => GraphicsHelper.DrawLevelUpImage(Level, Name, Uid, ProfileTheme);

        public void DrawProfileImage(uint rank) => GraphicsHelper.DrawProfileImage(Level, Name, Uid, Exp, Pen, rank, ProfileTheme, CalculateExpBar());

        public void DrawRankImage(uint rank) => GraphicsHelper.DrawRankImage(Level, Name, Uid, rank, ProfileTheme, CalculateExpBar());

        public bool HasLeveledUp(uint length)
        {
            byte initalLevel = Level;
            Exp += (length * 7) % 300;
            byte newLevel = CalculateLevel();

            if (newLevel != initalLevel)
            {
                Pen += CalculatePenGain();
                Level = newLevel;

                return true;
            }

            UpdateUserAsync();

            return false;
        }

        public async void UpdateUserAsync() => await UserService.UpdateUserAsync(this);
    }
}
