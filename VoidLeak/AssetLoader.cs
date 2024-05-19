using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LethalLib.Modules;
using UnityEngine;

namespace VoidLeak;

public static class AssetLoader {
    private static AssetBundle? _assets;

    public static void LoadBundle() {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyLocation == null) {
            Plugin.logger.LogError("Failed to load asset bundle.");
            Plugin.logger.LogError("Check if the asset bundle is in the same directory as the plugin.");
            return;
        }

        _assets ??= AssetBundle.LoadFromFile(Path.Combine(assemblyLocation, "voidleak"));
    }

    public static IEnumerator LoadItems() {
        if (_assets is null)
            yield break;

        if (Plugin.configFile is null)
            yield break;

        var allAssets = _assets.LoadAllAssets<ItemWithDefaultWeight>();

        var selectableLevels = Resources.FindObjectsOfTypeAll<SelectableLevel>();

        var allItemsWithDefaultWeight = allAssets.OfType<ItemWithDefaultWeight>();

        var itemsWithDefaultWeight = allItemsWithDefaultWeight as ItemWithDefaultWeight[] ?? allItemsWithDefaultWeight.ToArray();

        RegisterDefaults(itemsWithDefaultWeight);

        RegisterBasedOnMoons(itemsWithDefaultWeight);

        Plugin.logger.LogInfo("All items were registered :)");
    }

    private static void RegisterBasedOnMoons(ItemWithDefaultWeight[] itemsWithDefaultWeight) {
        var configFile = Plugin.configFile;

        if (configFile is null)
            return;

        foreach (var levelType in Enum.GetValues(typeof(Levels.LevelTypes)).OfType<Levels.LevelTypes>()) {
            if (levelType is Levels.LevelTypes.None or Levels.LevelTypes.All or Levels.LevelTypes.Vanilla)
                continue;

            foreach (var item in itemsWithDefaultWeight) {
                var levelWeight = configFile.Bind(levelType.ToString(), $"{item.item.itemName} Spawn Weight",
                                                  item.defaultWeight,
                                                  $"Defines the {item.item.itemName} spawn weight on Planet {
                                                      levelType.ToString()[..(levelType.ToString().Length - 5)]}");

                Items.RegisterScrap(item.item, levelWeight.Value, levelType);
            }
        }
    }

    private static void RegisterDefaults(ItemWithDefaultWeight[] itemsWithDefaultWeight) {
        var configFile = Plugin.configFile;

        if (configFile is null)
            return;

        foreach (var item in itemsWithDefaultWeight) {
            var defaultSpawnWeight = configFile.Bind("1. Default", $"1. {item.item.itemName} Spawn Weight", item.defaultWeight,
                                                     "Defines the default spawn weight");

            var maxValue = configFile.Bind("1. Default", $"2. {item.item.itemName} Maximum Value", item.item.maxValue,
                                           "Defines the maximum scrap value.");

            var minValue = configFile.Bind("1. Default", $"3. {item.item.itemName} Minimum Value", item.item.minValue,
                                           "Defines the minimum scrap value.");


            item.defaultWeight = defaultSpawnWeight.Value;
            item.item.maxValue = maxValue.Value;
            item.item.minValue = minValue.Value;

            NetworkPrefabs.RegisterNetworkPrefab(item.item.spawnPrefab);
        }
    }
}