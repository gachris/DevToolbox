using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Media;
using DevToolbox.Wpf.Media;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Interop;

internal static class ActiveUserThemeReader
{
    [DllImport("Wtsapi32.dll", SetLastError = true)]
    static extern bool WTSEnumerateSessions(
        IntPtr hServer,
        int Reserved,
        int Version,
        out IntPtr ppSessionInfo,
        out int pCount
    );

    [DllImport("Wtsapi32.dll")]
    static extern void WTSFreeMemory(IntPtr pointer);

    [DllImport("Wtsapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool WTSQuerySessionInformation(
        IntPtr hServer,
        int sessionId,
        WTS_INFO_CLASS infoClass,
        out IntPtr ppBuffer,
        out int pBytesReturned
    );

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool LookupAccountName(
        string? lpSystemName,
        string lpAccountName,
        byte[] Sid,
        ref uint cbSid,
        StringBuilder ReferencedDomainName,
        ref uint cchReferencedDomainName,
        out SID_NAME_USE peUse
    );

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern int RegLoadKey(UIntPtr hKey, string lpSubKey, string lpFile);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern int RegUnLoadKey(UIntPtr hKey, string lpSubKey);

    enum WTS_INFO_CLASS
    {
        WTSUserName = 5,
        WTSDomainName = 7
    }

    enum SID_NAME_USE
    {
        SidTypeUser = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WTS_SESSION_INFO
    {
        public int SessionID;
        public IntPtr pWinStationName;
        public WTS_CONNECTSTATE_CLASS State;
    }

    enum WTS_CONNECTSTATE_CLASS
    {
        WTSActive,
        WTSConnected,
        WTSConnectQuery,
        WTSShadow,
        WTSDisconnected,
        WTSIdle,
        WTSListen,
        WTSReset,
        WTSDown,
        WTSInit
    }

    public static ApplicationTheme GetThemeForActiveUser()
    {
        int sessionId = GetActiveSessionId();
        string user = QuerySessionString(sessionId, WTS_INFO_CLASS.WTSUserName);
        string domain = QuerySessionString(sessionId, WTS_INFO_CLASS.WTSDomainName);
        string sid = AccountNameToSid($"{domain}\\{user}");

        string userProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace("Default", user));
        LoadUserHiveIfNeeded(sid, userProfilePath);

        using var key = Registry.Users.OpenSubKey(
            $@"{sid}\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            false);

        var raw = key?.GetValue("AppsUseLightTheme") as int?;
        return (raw == 0) ? ApplicationTheme.Dark : ApplicationTheme.Light;
    }

    public static bool ShowAccentColorOnTitleBarsAndWindows()
    {
        int sessionId = GetActiveSessionId();
        string user = QuerySessionString(sessionId, WTS_INFO_CLASS.WTSUserName);
        string domain = QuerySessionString(sessionId, WTS_INFO_CLASS.WTSDomainName);
        string sid = AccountNameToSid($"{domain}\\{user}");

        string userProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace("Default", user));
        LoadUserHiveIfNeeded(sid, userProfilePath);

        using var registryKey = Registry.Users.OpenSubKey(sid + "\\Software\\Microsoft\\Windows\\DWM", writable: false);
        var raw = registryKey?.GetValue("ColorPrevalence") as int?;
        return raw == 1;
    }

    static int GetActiveSessionId()
    {
        if (!WTSEnumerateSessions(IntPtr.Zero, 0, 1, out var ppSessionInfo, out var count))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        try
        {
            int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            for (int i = 0; i < count; i++)
            {
                var si = Marshal.PtrToStructure<WTS_SESSION_INFO>(ppSessionInfo + i * dataSize);
                if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive)
                    return si.SessionID;
            }
        }
        finally
        {
            WTSFreeMemory(ppSessionInfo);
        }

        throw new Exception("No active session found.");
    }

    static string QuerySessionString(int sessionId, WTS_INFO_CLASS infoClass)
    {
        if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId, infoClass, out var pBuffer, out _))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        try
        {
            return Marshal.PtrToStringUni(pBuffer)
                   ?? throw new Win32Exception("Null result from WTSQuerySessionInformation");
        }
        finally
        {
            WTSFreeMemory(pBuffer);
        }
    }

    static string AccountNameToSid(string accountName)
    {
        uint sidSize = 0, domSize = 0;
        _ = LookupAccountName(null, accountName, null!, ref sidSize, null!, ref domSize, out _);

        if (Marshal.GetLastWin32Error() != 122)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        var sidBuf = new byte[sidSize];
        var domBuf = new StringBuilder((int)domSize);

        if (!LookupAccountName(null, accountName, sidBuf, ref sidSize, domBuf, ref domSize, out _))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return new SecurityIdentifier(sidBuf, 0).Value;
    }

    static void LoadUserHiveIfNeeded(string sid, string userProfilePath)
    {
        if (Registry.Users.OpenSubKey(sid) != null)
            return;

        string ntUserDat = Path.Combine(userProfilePath, "NTUSER.DAT");
        if (!File.Exists(ntUserDat))
            throw new FileNotFoundException("NTUSER.DAT not found", ntUserDat);

        int result = RegLoadKey((UIntPtr)0x80000003, sid, ntUserDat); // HKEY_USERS
        if (result != 0)
            throw new Win32Exception(result);
    }
}
