using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Enums;
using TestAccountCore;
using TestAccountCore.Dependencies;
using static TestAccountCore.Netcode;

namespace VoidLeak;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("TestAccount666.TestAccountCore", "1.0.0")]
[BepInDependency("evaisa.lethallib")]
[BepInDependency("imabatby.lethallevelloader", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin {
    public static ManualLogSource logger = null!;
    private static Harmony? _harmony;
    private static readonly Version _CurrentConfigVersion = "2.0.0".ParseVersion();
    private readonly ConfigDefinition _configVersionDefinition = new("0. Config Version", "Do not touch!");

    private void Awake() {
        logger = Logger;

        _harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        CompareConfigVersion();

        if (DependencyChecker.IsLobbyCompatibilityInstalled()) {
            logger.LogInfo("Found LobbyCompatibility Mod, initializing support :)");
            LobbyCompatibilitySupport.Initialize(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION,
                                                 CompatibilityLevel.Everyone, VersionStrictness.Minor);
        }

        var executingAssembly = Assembly.GetExecutingAssembly();

        ExecuteNetcodePatcher(executingAssembly);

        AssetLoader.LoadBundle(executingAssembly, "voidleak");
        logger.LogInfo("Loaded asset bundle. Registering items.");
        AssetLoader.LoadItems(Config);

        DevGunConfig.Initialize(Config);

        logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded, version {MyPluginInfo.PLUGIN_VERSION}");
    }

    private Version GetConfigVersion() {
        var configVersionEntry = Config.Bind(_configVersionDefinition, "1.0.0",
                                             new("The config version. Might reset config, if touched!"));

        return configVersionEntry.Value.ParseVersion();
    }

    private void SetConfigVersionEntry() =>
        Config.Bind(_configVersionDefinition, _CurrentConfigVersion.ToString(),
                    new("The config version. Might reset config, if touched!"));

    private void CompareConfigVersion() {
        try {
            var configVersion = GetConfigVersion();

            var compareTo = configVersion.CompareTo(_CurrentConfigVersion);

            switch (compareTo) {
                case > 0:
                    Logger.LogWarning("Found a more recent config! proceed with caution!");
                    return;
                case < 0:
                    Logger.LogWarning("Found an old config! Config will be reset!");
                    Config.Clear();
                    SetConfigVersionEntry();
                    Config.Save();
                    CompareConfigVersion();
                    return;
            }
        } catch (Exception exception) {
            Logger.LogError($"An error occurred while comparing config versions: {exception.Message}");
        }
    }
}