using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChipUtillo
{
    public class RPCManager
    {
        [DllImport("AsusSASystemInfoWin32.dll", EntryPoint = "InitializeFunc")]
        private static extern IntPtr AsusSASystemInfoInitializeFunc();

        [DllImport("AsusSASystemInfoWin32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern long BigDataServiceCallFunc(
          StringBuilder inputString,
          StringBuilder outputString,
          int size,
          IntPtr ptr);

        [DllImport("AsusProArtRpcClientWin32.dll", EntryPoint = "InitializeFunc")]
        private static extern IntPtr ProArtRPCInitializeFunc();

        [DllImport("AsusProArtRpcClientWin32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern long ProArtServiceCmdFunc(
          StringBuilder inputString,
          StringBuilder outputString,
          int size,
          IntPtr ptr);

        public static RpcResponse CallRpc(RPCType rpcType, int func, int subfunc, RPCParameter param)
        {
            StringBuilder inputString = new StringBuilder(System.Text.Json.JsonSerializer.Serialize((object)new RpcRequest()
            {
                function = func,
                version = 1,
                subfunction = subfunc,
                parameter = param
            }), 1024);
            StringBuilder outputString = new StringBuilder(4096);
            long num = 0;
            try
            {
                switch (rpcType)
                {
                    case RPCType.AsusProArtRpcClient:
                        IntPtr ptr1 = RPCManager.ProArtRPCInitializeFunc();
                        num = RPCManager.ProArtServiceCmdFunc(inputString, outputString, 4096, ptr1);
                        break;
                    case RPCType.AsusSASystemInfo:
                        IntPtr ptr2 = RPCManager.AsusSASystemInfoInitializeFunc();
                        num = RPCManager.BigDataServiceCallFunc(inputString, outputString, 4096, ptr2);
                        break;
                }
            }
            catch (DllNotFoundException ex)
            {
            }
            RpcResponse rpcResponse = (RpcResponse)null;
            try
            {
                rpcResponse = System.Text.Json.JsonSerializer.Deserialize<RpcResponse>(outputString.ToString());
            }
            catch (Exception ex)
            {
            }
            return rpcResponse;
        }
    }

    public enum ProArtRpcSubFunctions
    {
        MotherBoardExecutableCheck = 1,
        ProArtCheck_Manufacturer = 1,
        ProArtDT_SETFEATURE = 1,
        Toast_DisplayToast = 1,
        LaunchProcessWithAdmin = 2,
        MotherBoardExecutableLaunch = 2,
        ProArtCheck_EnclosureType = 2,
        ProArtDT_GETFEATURE = 2,
        Toast_DisplayToastCancel = 2,
        ProArtCheck_ProductName = 3,
        ProArtCheck_CAL1 = 4,
        ProArtCheck_CheckIsProArtLaptop = 5,
        System_CPUTemperature = 6,
        System_GPUFanRpm = 9,
    }

    public enum RPCError
    {
        None = 0,
        ServiceNoResponse = 3001, // 0x00000BB9
        Exception = 3002, // 0x00000BBA
        ServiceReturnResultEmpty = 3003, // 0x00000BBB
    }

    public enum RPCType
    {
        AsusProArtRpcClient,
        AsusSASystemInfo,
    }

    public class RPCParameter
    {
        public string p1;
        public string p2;
        public string p3;
        public string p4;
        public string p5;
    }

    public class RpcRequest
    {
        public int function;
        public int version;
        public int subfunction;
        public RPCParameter parameter;
    }

    public class RpcResponse
    {
        public int function { get; set; }

        public string parameter { get; set; }

        public string status { get; set; }

        public int version { get; set; }
    }

    public class RpcResult
    {
        public string result { get; set; }
    }
}
