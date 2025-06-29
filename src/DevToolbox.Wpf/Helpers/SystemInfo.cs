using System;

namespace DevToolbox.Wpf.Helpers;

internal class SystemInfo
{
    public static Lazy<Version> Version { get; private set; } = new Lazy<Version>(GetVersionInfo!);

    internal static Version? GetVersionInfo()
    {
        var registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\", false);
        if (registryKey == null) return default;

        var majorValue = registryKey.GetValue("CurrentMajorVersionNumber");
        var minorValue = registryKey.GetValue("CurrentMinorVersionNumber");
        var buildValue = (string?)registryKey.GetValue("CurrentBuild", 7600);
        var canReadBuild = int.TryParse(buildValue, out var build);

        var defaultVersion = Environment.OSVersion.Version;

        if (majorValue is int major && minorValue is int minor && canReadBuild)
        {
            return new Version(major, minor, build);
        }
        else
        {
            return new Version(defaultVersion.Major, defaultVersion.Minor, defaultVersion.Revision);
        }
    }

    internal static bool IsWin10()
    {
        return Version.Value.Major == 10;
    }

    internal static bool IsWin7()
    {
        return Version.Value.Major == 6 && Version.Value.Minor == 1;
    }

    internal static bool IsWin8x()
    {
        return Version.Value.Major == 6 && (Version.Value.Minor == 2 || Version.Value.Minor == 3);
    }
}