using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DiscordBot.Services
{
    public static class FileService
    {
        private static readonly ILogger Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger().ForContext(Constants.SourceContextPropertyName, nameof(FileService));

        public static void StartService()
        {
            var worker = new Thread(CleanFiles);
            worker.IsBackground = true;
            worker.Start();
        }

        private static void CleanFiles()
        {
            while (true)
            {
                Thread.Sleep(30000);

                var files = FindFiles();

                foreach (var file in files)
                    File.Delete(file);

                Logger.Information("[FileService] Cleaned {i} files.", files.Count);
            };
        }

        private static List<string> FindFiles()
        {
            var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "resources", "*.*", SearchOption.AllDirectories)
                .Where(s => s.Contains("profiletemp") || s.Contains("ranktemp") || s.Contains("leveltemp") || s.Contains("avatartemp"));

            return files.ToList();
        }
    }
}
