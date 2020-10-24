using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MumbleSharp;

namespace KNFA.Bots.MTB
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var processArch = RuntimeInformation.ProcessArchitecture;
            var isWindows = PlatformDetails.IsWindows;

            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory!;
            
            var (sourceLibPath, destinationLibPath) =
                (processArch, isWindows) switch
                {
                    (Architecture.X64, true) => (
                        Path.Combine(currentDirectory, "NativeDependencies", "opus.dll"),
                        Path.Combine(currentDirectory, "Audio", "Codecs", "Opus", "Libs", "64bit", "opus.dll")),
                    (Architecture.X64, false) => (
                        Path.Combine(currentDirectory, "NativeDependencies", "libopus.so.0"),
                        Path.Combine(currentDirectory, "Audio", "Codecs", "Opus", "Libs", "libopus.so.0")),
                    _ => throw new ApplicationException("Only linux64 and win64 are supported.")
                };

            if (!File.Exists(destinationLibPath))
            {
                var directory = Path.GetDirectoryName(destinationLibPath);
                Directory.CreateDirectory(directory);
            }

            File.Copy(sourceLibPath, destinationLibPath, true);

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureLogging(x => x.AddConsole())
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
