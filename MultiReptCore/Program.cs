using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;
using Newtonsoft.Json;

namespace MultiReptCore
{
    internal class Program
    {
        public static Config Config = new Config();

        public static string AppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MultiReptCore");

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            if (!Directory.Exists(AppData))
                Directory.CreateDirectory(AppData);

            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var conf = Path.Combine(location, "MultiReptCore.config.json");
            if (File.Exists(conf))
                LoadConfig(conf);

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();


        private static void LoadConfig(string confFilePath)
        {
            using (var strm = new FileStream(confFilePath, FileMode.Open))
            using (var reader = new StreamReader(strm, new UTF8Encoding(true)))
            {
                var body = reader.ReadToEnd();
                var conf = JsonConvert.DeserializeObject<Config>(body);

                try
                {
                    Thread.CurrentThread.CurrentUICulture =
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(conf.Locale);
                }
                catch { }


                Config = conf;
            }
        }
    }

    class Config
    {
        public string Locale { set; get; }

        public int HistoryBackupCount { set; get; }

        public Config()
        {
            Locale = "default";
            HistoryBackupCount = 20;
        }
    }
}
