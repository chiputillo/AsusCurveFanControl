using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChipUtillo
{
    public static class FanModeManager
    {
        private static string AsusOptimizationPath = "SOFTWARE\\ASUS\\ASUS System Control Interface\\AsusOptimization\\ASUS Keyboard Hotkeys";
        private static string FanModePath = "QuietFan";
        private static string FanSupportedPath = "QuietFanSupported";
        public static uint WM_ACPISIMULATION_MESSAGE = FanModeManager.RegisterWindowMessage("ACPI Simulation through ATKHotkey from Application");
        public static uint WM_ACPINOTIFICATION_MESSAGE = FanModeManager.RegisterWindowMessage("ACPI Notification through ATKHotkey from BIOS");

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint RegisterWindowMessage(string lpString);

        [DllImport("AsusATKHelper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool ACPI_ATK_DSTS(int arg, ref ulong returnValue);

        public static int GetFanSupport()
        {
            ulong num = 1507328;
            ulong returnValue = 0;
            FanModeManager.ACPI_ATK_DSTS(1114137, ref returnValue);
            return (int)((returnValue & num) >> 16);
        }

        public static FanModeManager.FanMode GetFanStatus()
        {
            int fanStatus = 0;
            FanModeManager.FindWindow("HCONTROL", "HControl");
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(FanModeManager.AsusOptimizationPath);
            if (registryKey != null)
                fanStatus = (int)registryKey.GetValue(FanModeManager.FanModePath, (object)0);
            return (FanModeManager.FanMode)fanStatus;
        }

        public static void SetFanStatus(FanModeManager.FanMode mode) => FanModeManager.PostMessage(FanModeManager.FindWindow("HCONTROL", "HControl"), FanModeManager.WM_ACPISIMULATION_MESSAGE, 157U, (uint)(128 + mode));

        public enum ProArtFanSubFunctions
        {
            System_CPUFanRpm = 1,
            System_GPUFanRpm = 2,
        }

        public enum FanMode
        {
            Normal = 0,
            Quiet = 1,
            Turbo = 2,
            FullSpeed = 3,
            None = 999, // 0x000003E7
        }
    }
}
