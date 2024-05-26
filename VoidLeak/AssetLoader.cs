using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using LethalLib.Modules;
using UnityEngine;

namespace VoidLeak;

public static class AssetLoader {
    private static AssetBundle? _assets;

    public static void LoadBundle() {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyLocation == null) {
            Plugin.logger.LogError("Failed to determine assembly location.");
            return;
        }

        var assetBundlePath = Path.Combine(assemblyLocation, "voidleak");
        if (!File.Exists(assetBundlePath)) {
            Plugin.logger.LogFatal(new StringBuilder($"Asset bundle not found at {assetBundlePath}.")
                                   .Append(" ")
                                   .Append("Check if the asset bundle is in the same directory as the plugin.")
                                   .ToString());
            return;
        }

        try {
            _assets = AssetBundle.LoadFromFile(assetBundlePath);
        } catch (Exception ex) {
            Plugin.logger.LogError($"Failed to load asset bundle: {ex.Message}");
        }
    }


    public static void LoadItems() {
        if (_assets is null || Plugin.configFile is null)
            return;

        var allAssets = _assets.LoadAllAssets<ItemWithDefaultWeight>();

        var allItemsWithDefaultWeight = allAssets.OfType<ItemWithDefaultWeight>();

        var itemsWithDefaultWeight = allItemsWithDefaultWeight.ToList();

        RegisterAllScrap(itemsWithDefaultWeight);

        Plugin.logger.LogInfo("All items were registered :)");
    }

    private static void RegisterAllScrap(List<ItemWithDefaultWeight> itemsWithDefaultWeight) {
        var configFile = Plugin.configFile;

        if (configFile is null)
            return;

        itemsWithDefaultWeight.ForEach(RegisterScrap);
    }

    private static void RegisterScrap(ItemWithDefaultWeight item) {
        var configFile = Plugin.configFile;

        if (configFile is null)
            return;

        var canItemSpawn = configFile.Bind($"{item.item.itemName}", "1. Enabled", true,
                                           $"If false, {item.item.itemName
                                           } will not be registered. This is different from a spawn weight of 0!");

        if (!canItemSpawn.Value)
            return;

        var maxValue = configFile.Bind($"{item.item.itemName}", "2. Maximum Value", item.item.maxValue,
                                       $"Defines the maximum scrap value for {item.item.itemName}.");

        var minValue = configFile.Bind($"{item.item.itemName}", "3. Minimum Value", item.item.minValue,
                                       $"Defines the minimum scrap value for {item.item.itemName}.");

        var configMoonRarity =
            configFile.Bind($"{item.item.itemName}", "4. Moon Spawn Weight", $"Vanilla:{item.defaultWeight}, Modded:{item.defaultWeight}",
                            $"Defines the spawn weight per moon. e.g. Assurance:{item.defaultWeight}");

        item.item.maxValue = maxValue.Value;
        item.item.minValue = minValue.Value;

        var parsedConfig = configMoonRarity.Value.ParseConfig();

        Items.RegisterScrap(item.item, parsedConfig.spawnRateByLevelType, parsedConfig.spawnRateByCustomLevelType);

        NetworkPrefabs.RegisterNetworkPrefab(item.item.spawnPrefab);
    }

    private static (Dictionary<Levels.LevelTypes, int> spawnRateByLevelType, Dictionary<string, int> spawnRateByCustomLevelType)
        ParseConfig(this string configMoonRarity) {
        Dictionary<Levels.LevelTypes, int> spawnRateByLevelType = [
        ];
        Dictionary<string, int> spawnRateByCustomLevelType = [
        ];

        foreach (var entry in configMoonRarity.Split(',').Select(configEntry => configEntry.Trim())) {
            if (string.IsNullOrWhiteSpace(entry))
                continue;

            string[] entryParts = entry.Split(':');

            if (entryParts.Length != 2)
                continue;

            var name = entryParts[0];

            if (!int.TryParse(entryParts[1], out var spawnWeight))
                continue;

            if (Enum.TryParse<Levels.LevelTypes>(name, true, out var levelType)) {
                spawnRateByLevelType[levelType] = spawnWeight;
                Plugin.logger.LogInfo($"Registered weight for level type {levelType} to {spawnWeight}");
                continue;
            }

            spawnRateByCustomLevelType[name] = spawnWeight;
            Plugin.logger.LogInfo($"Registered weight for custom level type {name} to {spawnWeight}");
        }

        return (spawnRateByLevelType, spawnRateByCustomLevelType);
    }
}