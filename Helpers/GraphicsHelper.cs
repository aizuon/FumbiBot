using System;
using System.Drawing;

namespace DiscordBot.Helpers
{
    public static class GraphicsHelper
    {
        public static void DrawLevelUpImage(uint level, string name, ulong uid, byte theme)
        {
            var image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\LevelUp" + theme + ".png");
            var icon = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\Level" + level.ToString() + ".png");
            using (var bmp = new Bitmap(285, 96))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(image, 0, 0, 285, 96);
                    g.DrawImage(icon, 23, 23, 50, 50);
                    using (var arialFont = new Font("Arial", 21, FontStyle.Bold))
                    {
                        g.DrawString(level.ToString(), arialFont, Brushes.White, 87, 43);
                        g.DrawString(name, arialFont, Brushes.White, 137, 45);
                    }
                }

                bmp.Save(AppDomain.CurrentDomain.BaseDirectory + "resources\\leveltemp_" + uid + ".png");
            }

            image.Dispose();
            icon.Dispose();
        }

        public static void DrawProfileImage(uint level, string name, ulong uid, uint exp, uint pen, uint rank, byte theme, User.ExpBar expBar)
        {
            var image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\Profile" + theme + ".png");
            var avatar = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\avatartemp_" + uid + ".png");
            var icon = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\Level" + level.ToString() + ".png");
            var expbar = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\ExpBar" + theme + ".png");
            using (var bmp = new Bitmap(285, 192))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(image, 0, 0, 285, 192);
                    g.DrawImage(avatar, 20, 20, 55, 55);
                    g.DrawImage(icon, 89, 35, 26, 26);
                    g.DrawImage(expbar, 42, 100, 200 * expBar.Percentage, 11);
                    using (var bottomFont = new Font("Arial", 9))
                    {
                        g.DrawString(level.ToString(), bottomFont, Brushes.White, 135 - MeasureString(level.ToString(), bottomFont).Width, 164);
                        g.DrawString(exp.ToString(), bottomFont, Brushes.White, 135 - MeasureString(exp.ToString(), bottomFont).Width, 142);
                        g.DrawString(pen.ToString(), bottomFont, Brushes.White, 262 - MeasureString(pen.ToString(), bottomFont).Width, 142);
                        g.DrawString(rank.ToString(), bottomFont, Brushes.White, 262 - MeasureString(rank.ToString(), bottomFont).Width, 164);
                        g.DrawString(((uint)(expBar.Percentage * 100)).ToString() + "%", bottomFont, Brushes.White, 275 - MeasureString(((uint)(expBar.Percentage * 100)).ToString() + "%", bottomFont).Width, 97);
                        g.DrawString("Exp:", bottomFont, Brushes.White, 16, 97);
                        g.DrawString("/", bottomFont, Brushes.White, 142, 114);
                        g.DrawString(expBar.CurrentExp.ToString(), bottomFont, Brushes.White, 144 - MeasureString(expBar.CurrentExp.ToString(), bottomFont).Width, 114);
                        g.DrawString(expBar.NextLevelExp.ToString(), bottomFont, Brushes.White, 147, 114);
                    }
                    using (var nameFont = new Font("Arial", 18, FontStyle.Bold))
                    {
                        g.DrawString(name, nameFont, Brushes.White, 128, 33);
                    }
                }

                bmp.Save(AppDomain.CurrentDomain.BaseDirectory + "resources\\profiletemp_" + uid + ".png");
            }

            image.Dispose();
            avatar.Dispose();
            icon.Dispose();
            expbar.Dispose();
        }

        public static void DrawRankImage(uint level, string name, ulong uid, uint rank, byte theme, User.ExpBar expBar)
        {
            var image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\Rank" + theme + ".png");
            var avatar = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\avatartemp_" + uid + ".png");
            var icon = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\Level" + level.ToString() + ".png");
            var expbar = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "resources\\ExpBar" + theme + ".png");
            using (var bmp = new Bitmap(285, 96))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(image, 0, 0, 285, 96);
                    g.DrawImage(avatar, 19, 19, 40, 40);
                    g.DrawImage(icon, 73, 26, 26, 26);
                    g.DrawImage(expbar, 38, 64, 200 * expBar.Percentage, 11);
                    using (var bottomFont = new Font("Arial", 9))
                    {
                        g.DrawString("Rank: #" + rank.ToString(), bottomFont, Brushes.White, 115, 47);
                        g.DrawString(((uint)(expBar.Percentage * 100)).ToString() + "%", bottomFont, Brushes.White, 270 - MeasureString(((uint)(expBar.Percentage * 100)).ToString() + "%", bottomFont).Width, 61);
                        g.DrawString("Exp:", bottomFont, Brushes.White, 13, 61);
                    }
                    using (var nameFont = new Font("Arial", 18, FontStyle.Bold))
                    {
                        g.DrawString(name, nameFont, Brushes.White, 115, 20);
                    }
                }

                bmp.Save(AppDomain.CurrentDomain.BaseDirectory + "resources\\ranktemp_" + uid + ".png");
            }

            image.Dispose();
            avatar.Dispose();
            icon.Dispose();
            expbar.Dispose();
        }

        private static SizeF MeasureString(string text, Font font)
        {
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return g.MeasureString(text, font);
            }
        }
    }
}
