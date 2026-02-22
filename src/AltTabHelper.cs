using System;
using System.Runtime.InteropServices;
using System.Threading;

public static class AltTabHelper
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const int KEYEVENTF_KEYUP = 0x0002;
    private const byte VK_MENU = 0x12;  // Alt
    private const byte VK_TAB = 0x09;   // Tab

    public static void PressAltTab()
    {
        // Press Alt down
        keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);

        // Press Tab
        keybd_event(VK_TAB, 0, 0, UIntPtr.Zero);
        Thread.Sleep(50); // small delay
        keybd_event(VK_TAB, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

        // Release Alt
        keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

        Thread.Sleep(200); // give target window time to activate
    }
}