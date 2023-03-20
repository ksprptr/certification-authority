using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace CABlazorApp.Services;

public class AlternateDataStream
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
 
    private const int FILE_ATTRIBUTE_HIDDEN = 0x2;
    private const int FILE_ATTRIBUTE_SYSTEM = 0x4;
 
    public static void WriteAds(string fileName, string adsName, string content)
    {
        string adsPath = $"{fileName}:{adsName}";
        using (SafeFileHandle handle = CreateFile(adsPath, FileAccess.Write, FileShare.None, IntPtr.Zero, FileMode.Create, FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM, IntPtr.Zero))
        {
            if (!handle.IsInvalid)
            {
                using (StreamWriter sw = new StreamWriter(new FileStream(handle, FileAccess.Write)))
                {
                    sw.Write(content);
                }
            }
            else
            {
                throw new IOException($"Failed to create or open the ADS '{adsName}' for '{fileName}'.", Marshal.GetLastWin32Error());
            }
        }
    }
 
    public static string ReadAds(string fileName, string adsName)
    {
        string adsPath = $"{fileName}:{adsName}";
        using (SafeFileHandle handle = CreateFile(adsPath, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM, IntPtr.Zero))
        {
            if (!handle.IsInvalid)
            {
                using (StreamReader sr = new StreamReader(new FileStream(handle, FileAccess.Read)))
                {
                    return sr.ReadToEnd();
                }
            }
            else
            {
                throw new IOException($"Failed to open the ADS '{adsName}' for '{fileName}'.", Marshal.GetLastWin32Error());
            }
        }
    }
}