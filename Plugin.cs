using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ItemManager;
using ServerSync;
using UnityEngine;

namespace PKTVendorRewards
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class PKTVendorRewards : BaseUnityPlugin
    {
        internal const string ModName = "PKTVendorRewards";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "kaepo";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        internal static string ConnectionError = "";

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource PKTVendorRewardsLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public void Awake()
        {
            _serverConfigLocked = config("General", "Force Server Config", true, "Force Server Config");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            Item PukeSpear = new("spear_puke", "Spear_Puke");
            PukeSpear.Name.English("Puke Spear"); 
            PukeSpear.Description.English("A Spear that makes you puke.");
            Item KinfeTarred = new("knife_tarred", "Knife_Tarred");
            KinfeTarred.Name.English("Tar Knife"); 
            KinfeTarred.Description.English("A Knife that makes you tarred.");

            //Item healthinstant = new("meadhealthinstant", "MeadHealthInstant");
            //healthinstant.Name.English("Instant Health Potion"); // You can use this to fix the display name in code
            //healthinstant.Description.English("75 instant health.");

            Item meadrun = new("meadrun", "MeadRun");
            meadrun.Name.English("Instant Run Speed Potion");
            meadrun.Description.English("10 run speed bonus.");

            Item meadblunt = new("meadblunt", "MeadBlunt");
            meadblunt.Name.English("Blunt Resistance");
            meadblunt.Description.English("Strong blunt resistance.");

            Item meadpierce = new("meadpierce", "MeadPierce");
            meadpierce.Name.English("Pierce Resistance"); 
            meadpierce.Description.English("Strong pierce resistance.");

            Item meadslash = new("meadslash", "MeadSlash");
            meadslash.Name.English("Slash Resistance");
            meadslash.Description.English("Strong slash resistance.");

            Item meadlightning = new("meadlightning", "MeadLightning");
            meadlightning.Name.English("Lightning Resistance"); 
            meadlightning.Description.English("Strong lightning resistance.");

            Item meadinvig = new("meadinvigorated", "MeadInvigorated");
            meadinvig.Name.English("Instant Invigoration"); 
            meadinvig.Description.English("Like Rested but better.");

            GameObject runeslashvisual = ItemManager.PrefabManager.RegisterPrefab(PrefabManager.RegisterAssetBundle("meadslash"), "vfx_runeslash");

            //GameObject axeSound =
            //   ItemManager.PrefabManager.RegisterPrefab("ironfang", "axeSound"); // Same for special sound effects

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                PKTVendorRewardsLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                PKTVendorRewardsLogger.LogError($"There was an issue loading your {ConfigFileName}");
                PKTVendorRewardsLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<bool>? _serverConfigLocked;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        #endregion
    }
}