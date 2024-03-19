using AsusFanControl;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace ChipUtillo
{
    public partial class Form1 : Form
    {
        bool initialized = false;
        bool applyCurve = false;
        int fanNum = 1;
        int rpmReadCycle = 0;
        int[] skipTempDecrease = null;
        List<Point> curvePercentPoints = null;
        int[] latestFanPercent = null;
        int[] lastFanRpm = null;
        AsusControl asusCtrl = new AsusControl();
        FanModeManager.FanMode prevFanMod = FanModeManager.FanMode.Normal;
        private bool Balanced_Standard_Flag;

        [DllImport("AsusATKHelper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern bool ACPI_ATK_DSTS(int arg, ref ulong returnValue);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateEvent(
          IntPtr lpEventAttributes,
          bool bManualReset,
          bool bInitialState,
          string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern int WaitForSingleObject(IntPtr handle, int milliseconds);

        public Form1()
        {
            InitializeComponent();

            if (File.Exists("pcur.ve"))
            {
                using (StreamReader sw = new StreamReader("pcur.ve"))
                {
                    curvePercentPoints = System.Text.Json.JsonSerializer.Deserialize<List<Point>>(sw.ReadToEnd());
                    fanCurve.SetPercentPoints(curvePercentPoints);
                    //applyCurve = true;
                }
            }
            Task.Run(() =>
            {
                prevFanMod = FanModeManager.GetFanStatus();

                fanNum = asusCtrl.HealthyTable_FanCounts();
                latestFanPercent = new int[fanNum];
                lastFanRpm = new int[fanNum];
                skipTempDecrease=new int[fanNum];

                for (int i = 0; i < fanNum; i++)
                {
                    lastFanRpm[i] = asusCtrl.GetFanSpeed((byte)i);
                }

                initialized = true;
            });
            //int fanSupport = FanModeManager.GetFanSupport();

            //lblCurrentMode.Text = string.Empty;

            //if (fanSupport >= 16)
            //{
            //    this.Balanced_Standard_Flag = true;
            //    lblCurrentMode.Text += "Balanced mode, ";
            //    fanSupport -= 16;
            //}
            //if (fanSupport >= 4)
            //{
            //    lblCurrentMode.Text += "Full speed mode, ";
            //    fanSupport -= 4;
            //}
            //if (fanSupport >= 2)
            //{
            //    lblCurrentMode.Text += "Performance mode, ";
            //    fanSupport -= 2;
            //}
            //if (fanSupport < 1)
            //{
            //    lblCurrentMode.Text += "Standard mode, ";
            //    return;
            //}
            //lblCurrentMode.Text += "Whisper mode, ";
            //int num = fanSupport - 1;


        }

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

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Task t = null;
            if (applyCurve)
            {
                t = Task.Run(() =>
                {
                    asusCtrl.SetFanSpeeds(0);

                });
            }
            if (curvePercentPoints != null && curvePercentPoints.Count > 0)
            {
                using (StreamWriter sw = new StreamWriter("pcur.ve", false))
                {
                    sw.Write(System.Text.Json.JsonSerializer.Serialize(curvePercentPoints));
                }
            }

            FanModeManager.SetFanStatus(prevFanMod);

            if (t != null)
            {
                t.Wait();
            }
            asusCtrl.Dispose();
        }

        private void btnApplyCurve_Click(object sender, EventArgs e)
        {
            curvePercentPoints = fanCurve.GetPercentPoints().OrderByDescending(o => o.X).ToList();
            applyCurve = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!initialized) return;
            Task.Run(async () =>
            {
                string m1 = string.Empty;
                Stopwatch sw = new Stopwatch();
                for (int i = 0; i < fanNum; i++)
                {
                    if (++rpmReadCycle >= 3)
                    {
                        rpmReadCycle = 0;
                        sw.Start();
                        lastFanRpm[i] = asusCtrl.GetFanSpeed((byte)i);
                        sw.Stop();
                    }
                    m1 += $"Fan {i}: {lastFanRpm[i]} rpm ({sw.Elapsed.Milliseconds} ms){Environment.NewLine}";

                }

                Tuple<int, string> temp = null;
                sw.Restart();
                int tmp = (int)asusCtrl.Thermal_Read_Cpu_Temperature();
                sw.Stop();

                temp = new Tuple<int, string>(tmp, $"Temp: {tmp}℃ ({sw.ElapsedMilliseconds} ms){Environment.NewLine}");

                string msg = m1 + temp.Item2;

                if (applyCurve && curvePercentPoints != null && curvePercentPoints.Count > 0)
                {
                    Point minPoint = curvePercentPoints.Last();
                    if (temp.Item1 <= minPoint.X)
                    {
                        sw.Reset();
                        for (int i = 0; i < fanNum; i++)
                        {
                            if (latestFanPercent[i] != minPoint.Y)
                            {
                                sw.Restart();
                                asusCtrl.SetFanSpeed(minPoint.Y, (byte)i);
                                sw.Stop();

                                latestFanPercent[i] = minPoint.Y;
                            }
                            msg += $"; Fan {i}: {minPoint.Y}% ({sw.ElapsedMilliseconds} ms){Environment.NewLine}";
                        }
                    }
                    else
                    {
                        sw.Reset();
                        foreach (Point curvePoint in curvePercentPoints)
                        {
                            if (temp.Item1 >= curvePoint.X)
                            {
                                for (int i = 0; i < fanNum; i++)
                                {
                                    if (latestFanPercent[i] != curvePoint.Y)
                                    {
                                        if (curvePoint.Y > latestFanPercent[i] || (curvePoint.Y < latestFanPercent[i] && skipTempDecrease[i] > 3))
                                        {
                                            skipTempDecrease[i] = 0;

                                            sw.Restart();
                                            asusCtrl.SetFanSpeed(curvePoint.Y, (byte)i);
                                            sw.Stop();

                                            latestFanPercent[i] = curvePoint.Y;

                                            msg += $"Fan {i}: {curvePoint.Y}% ({sw.ElapsedMilliseconds} ms){Environment.NewLine}";
                                        }
                                        else
                                        {
                                            if (curvePoint.Y < latestFanPercent[i])
                                            {
                                                skipTempDecrease[i]++;
                                            }
                                            msg += $"Fan {i}: {latestFanPercent[i]}% ({sw.ElapsedMilliseconds} ms){Environment.NewLine}";
                                        }
                                    }
                                    else
                                    {
                                        
                                        msg += $"Fan {i}: {latestFanPercent[i]}% ({sw.ElapsedMilliseconds} ms){Environment.NewLine}";
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                this.BeginInvoke(new Action(() =>
                {
                    lblCurrentMode.Text = msg.TrimEnd('\r', '\n'); ;
                }));
            });
        }
    }
}
