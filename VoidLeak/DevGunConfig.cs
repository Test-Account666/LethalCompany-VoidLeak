using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using VoidLeak.NetworkBehaviours;

namespace VoidLeak;

public static class DevGunConfig {
    public static readonly HashSet<DevGun.DevGunAction> ActionSet = [
        DevGun.DevGunAction.MULTIPLY, DevGun.DevGunAction.DIVIDE, DevGun.DevGunAction.DESTROY, DevGun.DevGunAction.SUBTRACT,
        DevGun.DevGunAction.ADD,
    ];

    private static ConfigEntry<int>? _divideWeight;
    private static ConfigEntry<int>? _multiplyWeight;
    private static ConfigEntry<int>? _destroyWeight;
    private static ConfigEntry<int>? _subtractWeight;
    private static ConfigEntry<int>? _addWeight;

    private static ConfigEntry<int>? _divideMinimum;
    private static ConfigEntry<int>? _multiplyMinimum;
    private static ConfigEntry<int>? _subtractMinimum;
    private static ConfigEntry<int>? _addMinimum;

    private static ConfigEntry<int>? _divideMaximum;
    private static ConfigEntry<int>? _multiplyMaximum;
    private static ConfigEntry<int>? _subtractMaximum;
    private static ConfigEntry<int>? _addMaximum;

    internal static void Initialize(ConfigFile configFile) {
        _divideWeight = configFile.Bind("Dev Manipulator", "6. Divide Weight", DevGun.DevGunAction.DIVIDE.GetWeight(),
                                        "The higher the weight, the more common");
        _multiplyWeight = configFile.Bind("Dev Manipulator", "7. Multiply Weight", DevGun.DevGunAction.MULTIPLY.GetWeight(),
                                          "The higher the weight, the more common");
        _destroyWeight = configFile.Bind("Dev Manipulator", "8. Destroy Weight", DevGun.DevGunAction.DESTROY.GetWeight(),
                                         "The higher the weight, the more common");
        _subtractWeight = configFile.Bind("Dev Manipulator", "9. Subtract Weight", DevGun.DevGunAction.SUBTRACT.GetWeight(),
                                          "The higher the weight, the more common");
        _addWeight = configFile.Bind("Dev Manipulator", "10. Add Weight", DevGun.DevGunAction.ADD.GetWeight(),
                                     "The higher the weight, the more common");


        _divideMinimum = configFile.Bind("Dev Manipulator", "11. Divide Minimum", DevGun.DevGunAction.DIVIDE.GetMinimum(),
                                         "The minimum operand to use");
        _divideMaximum = configFile.Bind("Dev Manipulator", "12. Divide Maximum", DevGun.DevGunAction.DIVIDE.GetMaximum(),
                                         "The maximum operand to use");

        _multiplyMinimum = configFile.Bind("Dev Manipulator", "13. Multiply Minimum", DevGun.DevGunAction.MULTIPLY.GetMinimum(),
                                           "The minimum operand to use");
        _multiplyMaximum = configFile.Bind("Dev Manipulator", "14. Multiply Maximum", DevGun.DevGunAction.MULTIPLY.GetMaximum(),
                                           "The maximum operand to use");


        _subtractMinimum = configFile.Bind("Dev Manipulator", "15. Subtract Minimum", DevGun.DevGunAction.SUBTRACT.GetMinimum(),
                                           "The minimum operand to use");
        _subtractMaximum = configFile.Bind("Dev Manipulator", "16. Subtract Maximum", DevGun.DevGunAction.SUBTRACT.GetMaximum(),
                                           "The maximum operand to use");


        _addMinimum = configFile.Bind("Dev Manipulator", "17. Add Minimum", DevGun.DevGunAction.ADD.GetMinimum(),
                                      "The minimum operand to use");
        _addMaximum = configFile.Bind("Dev Manipulator", "18. Add Maximum", DevGun.DevGunAction.ADD.GetMaximum(),
                                      "The maximum operand to use");
    }

    public static int GetWeight(this DevGun.DevGunAction devGunAction) =>
        devGunAction switch {
            DevGun.DevGunAction.DIVIDE => _divideWeight?.Value ?? 10,
            DevGun.DevGunAction.MULTIPLY => _multiplyWeight?.Value ?? 15,
            DevGun.DevGunAction.DESTROY => _destroyWeight?.Value ?? 12,
            DevGun.DevGunAction.SUBTRACT => _subtractWeight?.Value ?? 45,
            DevGun.DevGunAction.ADD => _addWeight?.Value ?? 65,
            var _ => throw new ArgumentOutOfRangeException(nameof(devGunAction), devGunAction, "Is not implemented, yet???"),
        };

    public static int GetMinimum(this DevGun.DevGunAction devGunAction) =>
        devGunAction switch {
            DevGun.DevGunAction.DIVIDE => _divideMinimum?.Value ?? 2,
            DevGun.DevGunAction.MULTIPLY => _multiplyMinimum?.Value ?? 2,
            DevGun.DevGunAction.SUBTRACT => _subtractMinimum?.Value ?? 30,
            DevGun.DevGunAction.ADD => _addMinimum?.Value ?? 30,
            var _ => throw new ArgumentOutOfRangeException(nameof(devGunAction), devGunAction, "Is not implemented, yet???"),
        };

    public static int GetMaximum(this DevGun.DevGunAction devGunAction) =>
        devGunAction switch {
            DevGun.DevGunAction.DIVIDE => _divideMinimum?.Value ?? 4,
            DevGun.DevGunAction.MULTIPLY => _multiplyMinimum?.Value ?? 4,
            DevGun.DevGunAction.SUBTRACT => _subtractMinimum?.Value ?? 56,
            DevGun.DevGunAction.ADD => _addMinimum?.Value ?? 56,
            var _ => throw new ArgumentOutOfRangeException(nameof(devGunAction), devGunAction, "Is not implemented, yet???"),
        };
}