using System;
using System.Globalization;

namespace VoidLeak;

public class Version(int major, int minor, int patch) : IComparable<Version> {
    private readonly int _major = major;
    private readonly int _minor = minor;
    private readonly int _patch = patch;

    public int CompareTo(Version? other) {
        if (other is null)
            throw new ArgumentNullException(nameof(other), "Cannot compare to null!");

        if (ReferenceEquals(this, other))
            return 0;

        var majorComparison = _major.CompareTo(other._major);

        switch (majorComparison) {
            case < 0:
                Plugin.logger.LogDebug($"Other major was bigger: {_major} -> {other._major}");
                break;
            case > 0:
                Plugin.logger.LogDebug($"Other major was smaller: {_major} -> {other._major}");
                break;
        }


        if (majorComparison != 0)
            return majorComparison;

        var minorComparison = _minor.CompareTo(other._minor);

        switch (minorComparison) {
            case < 0:
                Plugin.logger.LogDebug($"Other minor was bigger: {_major} -> {other._major}");
                break;
            case > 0:
                Plugin.logger.LogDebug($"Other minor was smaller: {_minor} -> {other._minor}");
                break;
        }

        if (minorComparison != 0)
            return minorComparison;

        var patchComparison = _patch.CompareTo(other._patch);

        switch (patchComparison) {
            case < 0:
                Plugin.logger.LogDebug($"Other patch was bigger: {_patch} -> {other._patch}");
                break;
            case > 0:
                Plugin.logger.LogDebug($"Other patch was smaller: {_patch} -> {other._patch}");
                break;
        }

        return patchComparison;
    }

    public override string ToString() => $"{_major}.{_minor}.{_patch}";
}

internal static class VersionParser {
    public static Version ParseVersion(this string versionString) {
        var splitString = versionString.Split(".");

        if (splitString is not {
                Length: 3,
            }) throw new ArgumentException("Version string must contain at three segments and cannot be null.", nameof(versionString));

        if (!int.TryParse(splitString[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var major))
            throw new FormatException($"Invalid format in version string: {versionString}");

        if (!int.TryParse(splitString[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minor))
            throw new FormatException($"Invalid format in version string: {versionString}");

        if (!int.TryParse(splitString[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var patch))
            throw new FormatException($"Invalid format in version string: {versionString}");

        return new(major, minor, patch);
    }
}