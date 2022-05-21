using MelonLoader;
using System;
using Discord;
using System.Threading;

[assembly: MelonInfo(typeof(MelonLoaderDiscordStatus.DiscordStatusPlugin), "Discord Status", "1.0.0", "SlidyDev")]
[assembly: MelonColor(ConsoleColor.DarkCyan)]

namespace MelonLoaderDiscordStatus
{
    public class DiscordStatusPlugin : MelonPlugin
    {
        public const long AppId = 977473789854089226;
        public Discord.Discord discordClient;
        public ActivityManager activityManager;

        private bool gameClosing;
        public bool GameStarted { get; private set; }
        public long gameStartedTime;

        public override void OnPreInitialization()
        {
            DiscordLibraryLoader.LoadLibrary();
            InitializeDiscord();
            UpdateActivity();
            new Thread(DiscordLoopThread).Start();
        }

        public override void OnApplicationLateStart()
        {
            GameStarted = true;
            gameStartedTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            UpdateActivity();
        }

        public override void OnApplicationQuit()
        {
            gameClosing = true;
        }

        public void DiscordLoopThread()
        {
            for (; ; )
            {
                if (gameClosing)
                    break;

                discordClient.RunCallbacks();
                Thread.Sleep(200);
            }
        }

        public void InitializeDiscord()
        {
            discordClient = new Discord.Discord(AppId, (ulong)CreateFlags.NoRequireDiscord);
            discordClient.SetLogHook(LogLevel.Debug, DiscordLogHandler);

            activityManager = discordClient.GetActivityManager();
        }

        private void DiscordLogHandler(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Info:
                case LogLevel.Debug:
                    LoggerInstance.Msg(message);
                    break;

                case LogLevel.Warn:
                    LoggerInstance.Warning(message);
                    break;

                case LogLevel.Error:
                    LoggerInstance.Error(message);
                    break;
            }
        }

        public void UpdateActivity()
        {
            var activity = new Activity
            {
                Details = $"Playing {MelonUtils.CurrentGameAttribute.Name}"
            };

            activity.Assets.LargeImage = "ml_icon";
            activity.Name = $"MelonLoader {BuildInfo.Version}";
            activity.Instance = true;
            activity.Assets.LargeText = activity.Name;

            var modsCount = MelonHandler.Mods.Count;
            activity.State = GameStarted ? $"{modsCount} {(modsCount == 1 ? "Mod" : "Mods")} Loaded" : "Loading MelonLoader";

            if (GameStarted)
                activity.Timestamps.Start = gameStartedTime;

            activityManager.UpdateActivity(activity, ResultHandler);
        }

        public void ResultHandler(Result result)
        {

        }
    }
}
