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
            uint fails;

            while (true)
            {
                Thread.Sleep(30000);

                var files = FindFiles();

                fails = 0;

                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        fails++;
                        Logger.Error("[FileService] Couldn't clean the file {file} (IOException)", file);
                    }
                }

                if (files.Count > 0)
                    Logger.Information("[FileService] Cleaned {i} files.", files.Count - fails);
            };
        }

        private static List<string> FindFiles()
        {
            var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "resources", "*.*", SearchOption.AllDirectories)
                .Where(s => s.Contains("profiletemp") || s.Contains("ranktemp") || s.Contains("leveltemp") || s.Contains("avatartemp") || s.Contains("dailytemp"));

            return files.ToList();
        }
    }
}
