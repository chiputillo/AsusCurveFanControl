using AsusFanControl;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace ChipUtillo
{
    public partial class Form1 : Form
    {
        volatile bool initialized = false;
        bool applyCurve = false;
        bool exit = false;
        int fanNum = 1;
        int rpmReadCycle = 0;
        volatile int lastTemperature = 0;
        int angleShift = 1;
        int[] skipTempDecrease = null;
        List<Point> curvePercentPoints = null;
        int[] latestFanPercent = null;
        int[] lastFanRpm = null;
        public static AsusControl asusCtrl = new AsusControl();
        public static FanModeManager.FanMode prevFanMod = FanModeManager.FanMode.Normal;
        private bool Balanced_Standard_Flag;
        GraphicsPath gpath = new GraphicsPath();
        Color ibgcolor = Color.FromArgb(64, Color.Indigo);
        Font iconFont = null;
        string stmp = null;
        SizeF size;
        DateTime twoSeconds = DateTime.UtcNow;

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

            iconFont = new Font(this.Font.FontFamily, 40.0f, FontStyle.Regular);

            UpdateIcon();


            if (File.Exists("pcur.ve"))
            {
                using (StreamReader sw = new StreamReader("pcur.ve"))
                {
                    curvePercentPoints = System.Text.Json.JsonSerializer.Deserialize<List<Point>>(sw.ReadToEnd());
                    fanCurve.SetPercentPoints(curvePercentPoints);
                    applyCurve = true;
                }
            }
            Task.Run(() =>
            {
                prevFanMod = FanModeManager.GetFanStatus();

                fanNum = asusCtrl.HealthyTable_FanCounts();
                latestFanPercent = new int[fanNum];
                lastFanRpm = new int[fanNum];
                skipTempDecrease = new int[fanNum];

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

        private void UpdateIcon()
        {

            int _width = 64;
            int _height = 64;
            int lt_cache = lastTemperature;
            Icon ticon = null;

            using (Bitmap tbmp = new Bitmap(_width, _height))
            {
                using (Graphics igfx = Graphics.FromImage(tbmp))
                {
                    igfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    igfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    igfx.SmoothingMode = SmoothingMode.AntiAlias;
                    igfx.Clear(ibgcolor);
                    if (latestFanPercent != null)
                    {
                        int centerX = _width / 2;
                        int centerY = _height / 2;

                        float radius = Math.Min(_width, _height) / 1.5f;

                        // Draw fan blades
                        int n = 0;
                        do
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                float startAngle = (i * (360f / 4)) + angleShift + (n * 2); // Start angle of each fan blade
                                float endAngle = startAngle + 60f; // End angle of each fan blade

                                // Define points of the fan blade
                                double outerRadius = radius * 0.9; // Adjust slightly smaller than the fan's radius 

                                Point p1 = new Point(centerX, centerY);
                                Point p2 = new Point(
                                            (int)(centerX + radius * Math.Cos(startAngle * Math.PI / 180.0)),
                                            (int)(centerY + radius * Math.Sin(startAngle * Math.PI / 180.0)));

                                // Control points for rounding - adjust offset as needed
                                Point cp1 = new Point(
                                            (int)(centerX + radius * 0.8 * Math.Cos((startAngle + 10) * Math.PI / 180.0)),
                                            (int)(centerY + radius * 0.8 * Math.Sin((startAngle + 10) * Math.PI / 180.0)));
                                Point cp2 = new Point(
                                            (int)(centerX + radius * 0.85 * Math.Cos((endAngle - 5) * Math.PI / 180.0)), // Adjust slightly
                                            (int)(centerY + radius * 0.85 * Math.Sin((endAngle - 5) * Math.PI / 180.0))); // Adjust slightly

                                // More control points for a smoother curve (adjust as needed)
                                Point midPoint = new Point(
                                                (int)(centerX + radius * 0.7 * Math.Cos(((startAngle + endAngle) / 2) * Math.PI / 180.0)),
                                                (int)(centerY + radius * 0.7 * Math.Sin(((startAngle + endAngle) / 2) * Math.PI / 180.0)));
                                Point cp3 = new Point(
                                            (int)(centerX + radius * 0.8 * Math.Cos(((endAngle - 10) * Math.PI / 180.0))), // Adjust slightly inward
                                            (int)(centerY + radius * 0.8 * Math.Sin(((endAngle - 10) * Math.PI / 180.0))));
                                Point cp4 = new Point(
                                            (int)(centerX + radius * 0.9 * Math.Cos(endAngle * Math.PI / 180.0)), // Adjust slightly outward
                                            (int)(centerY + radius * 0.9 * Math.Sin(endAngle * Math.PI / 180.0)));

                                // Create a path for filling 
                                gpath.Reset();
                                gpath.AddBezier(p1, cp1, cp2, p2);
                                gpath.AddBezier(cp3, p2, p2, p1);  // Adjusted control points for smoother tip, possibly a different order
                                gpath.CloseFigure();

                                // Fill the blade shape
                                igfx.FillPath(Brushes.DarkViolet, gpath);
                            }
                        } while (latestFanPercent[0] >= 80 && ++n < 4);
                        // Draw the central circle
                        int centralCircleRadius = (int)(radius / 5);
                        Rectangle centralCircleRect = new Rectangle(centerX - centralCircleRadius, centerY - centralCircleRadius,
                                                                     centralCircleRadius * 2, centralCircleRadius * 2);
                        igfx.FillEllipse(Brushes.DarkViolet, centralCircleRect);
                    }

                    stmp = lt_cache <= 0 ? "??" : lt_cache.ToString();
                    size = igfx.MeasureString(stmp, iconFont);
                    igfx.DrawString(stmp, iconFont, Brushes.White, _width / 2.0f - size.Width / 2.0f, _height / 2.0f - size.Height / 2.0f);

                    ticon = tbmp.BitmapToIcon();

                }
            }
            var prev_ico = notifyIcon1.Icon;
            notifyIcon1.Icon = ticon;
            if(prev_ico != null)
            {
                prev_ico.Dispose();
            }
        }
        void SaveAsIcon(Bitmap SourceBitmap, Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            // ICO header
            stream.WriteByte(0); stream.WriteByte(0);
            stream.WriteByte(1); stream.WriteByte(0);
            stream.WriteByte(1); stream.WriteByte(0);

            // Image size
            stream.WriteByte((byte)SourceBitmap.Width);
            stream.WriteByte((byte)SourceBitmap.Height);
            // Palette
            stream.WriteByte(0);
            // Reserved
            stream.WriteByte(0);
            // Number of color planes
            stream.WriteByte(0); stream.WriteByte(0);
            // Bits per pixel
            stream.WriteByte(32); stream.WriteByte(0);

            // Data size, will be written after the data
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);

            // Offset to image data, fixed at 22
            stream.WriteByte(22);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);

            // Writing actual data
            SourceBitmap.Save(stream, ImageFormat.Png);

            // Getting data length (file length minus header)
            long Len = stream.Length - 22;

            // Write it in the correct place
            stream.Seek(14, SeekOrigin.Begin);
            stream.WriteByte((byte)Len);
            stream.WriteByte((byte)(Len >> 8));
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

           
            if (iconFont != null) { iconFont.Dispose(); }
            if (gpath != null) { gpath.Dispose(); }
            if(notifyIcon1.Icon!=null) { try { notifyIcon1.Icon.Dispose(); } catch { } }
            
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
            if (DateTime.UtcNow.Subtract(twoSeconds).TotalSeconds >= 2)
            {
                twoSeconds = DateTime.UtcNow;
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
                    lastTemperature = (int)asusCtrl.Thermal_Read_Cpu_Temperature();
                    sw.Stop();

                    temp = new Tuple<int, string>(lastTemperature, $"Temp: {lastTemperature}℃ ({sw.ElapsedMilliseconds} ms){Environment.NewLine}");

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
            UpdateIcon();
            if (latestFanPercent != null && latestFanPercent[0] != 0)
            {
                angleShift = (int)((angleShift + (360 / 10) * (latestFanPercent[0] / 100.0f)) % 360);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exit = true;
            this.Close();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;

            }
            else
            {
                this.ShowInTaskbar = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exit && e.CloseReason != CloseReason.WindowsShutDown && e.CloseReason != CloseReason.ApplicationExitCall && e.CloseReason != CloseReason.TaskManagerClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
        }
    }
}
