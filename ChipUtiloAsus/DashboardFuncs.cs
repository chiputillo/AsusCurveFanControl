using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChipUtillo
{
    public class DashboardFuncs
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx(DashboardFuncs.MEMORYSTATUSEX lpBuffer);

        [DllImport("AsusATKHelper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool ACPI_ATK_DSTS(int arg, ref ulong returnValue);

        public void GetSystemFanInfo(
          FanModeManager.ProArtFanSubFunctions subfunc,
          ref SystemInfoObject systemInfo)
        {
            ulong num = 65536;
            ulong maxValue = (ulong)ushort.MaxValue;
            ulong returnValue = 0;
            switch (subfunc)
            {
                case FanModeManager.ProArtFanSubFunctions.System_CPUFanRpm:
                    DashboardFuncs.ACPI_ATK_DSTS(1114131, ref returnValue);
                    systemInfo.CPUFanRpm = ((returnValue & maxValue) * 100UL).ToString();
                    break;
                case FanModeManager.ProArtFanSubFunctions.System_GPUFanRpm:
                    DashboardFuncs.ACPI_ATK_DSTS(1114132, ref returnValue);
                    if (Convert.ToInt64(returnValue & num) != 0L)
                    {
                        systemInfo.GPUFanRpm = ((returnValue & maxValue) * 100UL).ToString();
                        break;
                    }
                    int systemInfo1 = (int)this.GetSystemInfo(ProArtRpcSubFunctions.System_GPUFanRpm, ref systemInfo);
                    break;
            }
        }

        public RPCError GetSystemInfo(ProArtRpcSubFunctions subfunc, ref SystemInfoObject systemInfo)
        {
            RPCError systemInfo1 = RPCError.None;
            RpcResponse rpcResponse = RPCManager.CallRpc(RPCType.AsusSASystemInfo, 20, (int)subfunc, (RPCParameter)null);
            if (rpcResponse == null || rpcResponse.parameter == null)
            {
                return RPCError.ServiceNoResponse;
            }
            string str = "";
            try
            {
                switch (subfunc)
                {
                    case ProArtRpcSubFunctions.System_CPUTemperature:
                        str = "System_CPUTemperature";
                        RpcResult rpcResult1 = System.Text.Json.JsonSerializer.Deserialize<RpcResult>(rpcResponse.parameter);
                        systemInfo.CPUTemperature = rpcResult1.result;
                        break;
                    case ProArtRpcSubFunctions.System_GPUFanRpm:
                        str = "System_GPUFanRpm";
                        RpcResult rpcResult2 = System.Text.Json.JsonSerializer.Deserialize<RpcResult>(rpcResponse.parameter);
                        systemInfo.GPUFanRpm = rpcResult2.result;
                        break;
                }
            }
            catch (Exception ex)
            {
                systemInfo1 = RPCError.Exception;
            }
            return systemInfo1;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength = 64;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }
    }

    public class SystemInfoObject
    {
        public string CPUFanRpm { get; set; }

        public string CPUFrequency { get; set; }

        public string CPUMaxFrequency { get; set; }

        public string CPUInfo { get; set; }

        public string CPUTemperature { get; set; }

        public string CPUUsage { get; set; }

        public string CPUVoltage { get; set; }

        public string GPUFanRpm { get; set; }

        public string GPUInfo { get; set; }

        public string GPUFrequency { get; set; }

        public string GPUMaxFrequency { get; set; }

        public string GPUTemperature { get; set; }

        public string GPUUsage { get; set; }

        public string GPUVoltage { get; set; }

        public string MemoryFrequency { get; set; }

        public string MemoryTotalSize { get; set; }

        public string MemoryUsage { get; set; }

        public SystemInfoObject()
        {
            this.CPUFanRpm = "0";
            this.CPUFrequency = "0";
            this.CPUMaxFrequency = "5000";
            this.CPUInfo = "";
            this.CPUTemperature = "0";
            this.CPUUsage = "0";
            this.CPUVoltage = "0";
            this.GPUFanRpm = "0";
            this.GPUInfo = "";
            this.GPUFrequency = "0";
            this.GPUMaxFrequency = "5000";
            this.GPUTemperature = "0";
            this.GPUUsage = "0";
            this.GPUVoltage = "0";
            this.MemoryFrequency = "0";
            this.MemoryTotalSize = "0";
            this.MemoryUsage = "0";
        }
    }
}
