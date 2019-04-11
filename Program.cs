namespace DiscordBot
{
    public static class Program
    {
        static void Main()
        {
            Bot.Start(Config.Instance.BotToken);
        }
    }
}
