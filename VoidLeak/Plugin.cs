using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VoidLeak;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("evaisa.lethallib")]
[BepInDependency("imabatby.lethallevelloader", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin {
    public static ManualLogSource logger = null!;
    public static ConfigFile? configFile;
    private static Harmony? _harmony;

    private void Awake() {
        logger = Logger;
        configFile = Config;

        _harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        if (DependencyChecker.IsLobbyCompatibilityInstalled()) {
            logger.LogInfo("Found LobbyCompatibility Mod, initializing support :)");
            LobbyCompatibilitySupport.Initialize();
        }

        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types) {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods) {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length <= 0)
                    continue;

                method.Invoke(null, null);
            }
        }

        AssetLoader.LoadBundle();
        logger.LogInfo("Loaded asset bundle. Registering items.");
        StartCoroutine(AssetLoader.LoadItems());

        logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded, version {MyPluginInfo.PLUGIN_VERSION}");
    }
}