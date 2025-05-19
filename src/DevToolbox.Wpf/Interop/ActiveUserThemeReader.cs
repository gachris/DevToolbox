using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using DevToolbox.Wpf.Media;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Interop;

internal static class ActiveUserThemeReader
{
    [DllImport("kernel32.dll")]
    static extern uint WTSGetActiveConsoleSessionId();

    [DllImport("Wtsapi32.dll",
        SetLastError = true,
        CharSet = CharSet.Unicode   // ← ensure we call WTSQuerySessionInformationW
    )]
    static extern bool WTSQuerySessionInformation(
        IntPtr hServer,
        uint sessionId,
        WTS_INFO_CLASS infoClass,
        out IntPtr ppBuffer,
        out uint pBytesReturned
    );

    [DllImport("Wtsapi32.dll")]
    static extern void WTSFreeMemory(IntPtr pointer);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool LookupAccountName(
        string lpSystemName,
        string lpAccountName,
        byte[] Sid,
        ref uint cbSid,
        StringBuilder ReferencedDomainName,
        ref uint cchReferencedDomainName,
        out SID_NAME_USE peUse);

    enum WTS_INFO_CLASS
    {
        WTSUserName = 5,
        WTSDomainName = 7
    }

    enum SID_NAME_USE
    {
        SidTypeUser = 1,
        // (others omitted)
    }

    public static ApplicationTheme GetThemeForActiveUser()
    {
        // 1) Which session is the console user on?
        uint sessionId = WTSGetActiveConsoleSessionId();
        if (sessionId == uint.MaxValue)
            throw new Win32Exception("No active console session.");

        // 2) Get <Domain> and <User> for that session
        string user = QuerySessionString(sessionId, WTS_INFO_CLASS.WTSUserName);
        string domain = QuerySessionString(sessionId, WTS_INFO_CLASS.WTSDomainName);

        // 3) Turn "<domain>\<user>" into a SID string
        string sid = AccountNameToSid($"{domain}\\{user}");

        // 4) Read their hive under HKEY_USERS\<SID>\...
        using var key = Registry.Users.OpenSubKey(
            $@"{sid}\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            false);

        var raw = key?.GetValue("AppsUseLightTheme") as int?;
        return (raw == 1) ? ApplicationTheme.Light : ApplicationTheme.Dark;
    }

    static string QuerySessionString(uint sessionId, WTS_INFO_CLASS infoClass)
    {
        if (!WTSQuerySessionInformation(
                IntPtr.Zero,
                sessionId,
                infoClass,
                out var pBuffer,
                out _))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        try
        {
            // Marshal as Unicode (UTF-16) instead of ANSI
            return Marshal.PtrToStringUni(pBuffer)
                   ?? throw new Win32Exception("Unexpected null from WTSQuerySessionInformation");
        }
        finally
        {
            WTSFreeMemory(pBuffer);
        }
    }

    static string AccountNameToSid(string accountName)
    {
        // First call to get required buffer sizes
        uint sidSize = 0, domSize = 0;
        _ = LookupAccountName(null!, accountName, null!, ref sidSize, null!, ref domSize, out _);

        if (Marshal.GetLastWin32Error() != 122) // ERROR_INSUFFICIENT_BUFFER
            throw new Win32Exception(Marshal.GetLastWin32Error());

        var sidBuf = new byte[sidSize];
        var domBuf = new StringBuilder((int)domSize);
        if (!LookupAccountName(null!, accountName, sidBuf, ref sidSize, domBuf, ref domSize, out _))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        // Convert to S-1-... string
        var sid = new SecurityIdentifier(sidBuf, 0);
        return sid.Value;
    }
}