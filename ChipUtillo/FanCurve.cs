using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChipUtillo
{
    public partial class FanCurve : UserControl
    {
        const int ptNum = 8;
        const float nonWorkingAreaSize = 30f;
        const float pointSize = 8f;
        const float dpiCoeff = -0.1f;

        PointF[] percentPoints = null;
        PointF[] curvePoints = new PointF[ptNum];

        int curvePointIndx = 0;
        bool isDragging = false;
        bool highlightCurvePoint = false;

        HatchBrush gridBrush = null;
        Pen gridPen = null;

        public FanCurve()
        {
            this.Disposed += new EventHandler(OnDispose);
            gridBrush = new HatchBrush(HatchStyle.Percent50, Color.White);
            gridPen = new Pen(gridBrush);

            percentPoints = System.Text.Json.JsonSerializer.Deserialize<PointF[]>(@"[{""X"":87,""Y"":100},{""X"":84,""Y"":85},{""X"":80,""Y"":64},{""X"":74,""Y"":50},{""X"":65,""Y"":43},{""X"":55,""Y"":40},{""X"":45,""Y"":38},{""X"":20,""Y"":35}]");

            InitializeComponent();
            this.WorkingWidth = this.Width - (nonWorkingAreaSize * 2);
            this.WorkingHeight = this.Height - (nonWorkingAreaSize * 2);
            this.DoubleBuffered = true;
            BackColor = Color.Black;
        }

        void OnDispose(object sender, EventArgs e)
        {
            if (gridBrush != null)
            {
                gridBrush.Dispose();
            }
            if (gridPen != null)
            {
                gridPen.Dispose();
            }
        }



        private float WorkingWidth { get; set; }
        private float WorkingHeight { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawString("fan rpm, %", this.Font, Brushes.White, 2, 0);
            e.Graphics.DrawString("Temp, ℃", this.Font, Brushes.White, this.Width - 56, this.Height - (nonWorkingAreaSize * 0.55f));
            for (int i = 0; i < 11; i++)
            {
                e.Graphics.DrawString(((i) * 10).ToString(), this.Font, Brushes.White, 
                    (this.WorkingWidth) / 10.0f * i + nonWorkingAreaSize * 0.5f , 
                    this.Height - nonWorkingAreaSize);
                e.Graphics.DrawString(((i) * 10).ToString(), this.Font, Brushes.White,
                    i == 0 ? nonWorkingAreaSize * 0.5f : 0.0f,
                    Height - ((this.WorkingHeight) / 10.0f * i) - nonWorkingAreaSize);

                e.Graphics.DrawLine(gridPen, 
                    this.WorkingWidth / 10.0f * i + nonWorkingAreaSize - (pointSize * dpiCoeff), 
                    this.Height - nonWorkingAreaSize, 
                    this.WorkingWidth / 10.0f * i + nonWorkingAreaSize - (pointSize * dpiCoeff), 
                    nonWorkingAreaSize);
                if (i < 10)
                {
                    e.Graphics.DrawLine(gridPen, 
                        nonWorkingAreaSize, 
                        this.Height - ((this.WorkingHeight) / 10.0f * i) - (nonWorkingAreaSize) + (pointSize * dpiCoeff), 
                        this.Width, 
                        this.Height - ((this.WorkingHeight) / 10.0f * i) - (nonWorkingAreaSize) + (pointSize * dpiCoeff));
                }
            }

            for (int i = 0; i < percentPoints.Length; i++)
            {
                curvePoints[i].X = nonWorkingAreaSize + this.WorkingWidth * (percentPoints[i].X / 100.0f);
                curvePoints[i].Y = (this.Height - nonWorkingAreaSize) - (this.WorkingHeight * (percentPoints[i].Y / 100.0f));
            }
            e.Graphics.DrawCurve(Pens.White, curvePoints);

            for (int i = 0; i < curvePoints.Length; i++)
            {
                if (highlightCurvePoint && i == curvePointIndx)
                {
                    e.Graphics.FillRectangle(Brushes.LimeGreen, curvePoints[i].X - pointSize * 0.5f, curvePoints[i].Y - pointSize * 0.5f, pointSize, pointSize);
                }
                else
                {
                    e.Graphics.FillRectangle(Brushes.White, curvePoints[i].X - pointSize * 0.5f, curvePoints[i].Y - pointSize * 0.5f, pointSize, pointSize);
                }
            }
            if (curvePointIndx >= 0 && curvePointIndx < curvePoints.Length)
            {
                e.Graphics.DrawString($"{Math.Truncate(percentPoints[curvePointIndx].X)} ℃ x {Math.Truncate(percentPoints[curvePointIndx].Y)}% rpm", this.Font, Brushes.LimeGreen, this.Width * 0.5f + 40, 2);
            }

            base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            curvePointIndx = -1;
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                for (int i = 0; i < curvePoints.Length; i++)
                {
                    if (e.X >= curvePoints[i].X - pointSize * 0.5f && e.X <= curvePoints[i].X + pointSize * 0.5f &&
                        e.Y >= curvePoints[i].Y - pointSize * 0.5f && e.Y <= curvePoints[i].Y + pointSize * 0.5f)
                    {
                        curvePointIndx = i;
                        this.Invalidate();
                        break;
                    }
                }
            }
            base.OnMouseEnter(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            isDragging = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool _invalidate = highlightCurvePoint;
            highlightCurvePoint = false;
            if (isDragging && curvePointIndx >= 0 && curvePointIndx < curvePoints.Length)
            {
                highlightCurvePoint = true;
                percentPoints[curvePointIndx].X = ((e.Location.X) - nonWorkingAreaSize) / this.WorkingWidth * 100.0f;
                if (percentPoints[curvePointIndx].X > 100) { percentPoints[curvePointIndx].X = 100; }
                if (percentPoints[curvePointIndx].X < 0) { percentPoints[curvePointIndx].X = 0; }

                percentPoints[curvePointIndx].Y = (this.WorkingHeight - (e.Location.Y - nonWorkingAreaSize)) / this.WorkingHeight * 100.0f;
                if (percentPoints[curvePointIndx].Y > 100) { percentPoints[curvePointIndx].Y = 100; }
                if (percentPoints[curvePointIndx].Y < 0) { percentPoints[curvePointIndx].Y = 0; }

                _invalidate = true;
            }
            else
            {
                
                for (int i = 0; i < curvePoints.Length; i++)
                {
                    if (e.X >= curvePoints[i].X - pointSize * 0.5f && e.X <= curvePoints[i].X + pointSize * 0.5f &&
                        e.Y >= curvePoints[i].Y - pointSize * 0.5f && e.Y <= curvePoints[i].Y + pointSize * 0.5f)
                    {
                        curvePointIndx = i;
                        highlightCurvePoint = true;
                        _invalidate = true;
                        break;
                    }
                }
            }

            if(_invalidate || highlightCurvePoint != _invalidate) 
            {
                this.Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            this.WorkingWidth = this.Width - (nonWorkingAreaSize * 2);
            this.WorkingHeight = this.Height - (nonWorkingAreaSize * 2);
            this.Invalidate();
            base.OnResize(e);
        }

        public List<Point> GetPercentPoints()
        {
            return percentPoints.Select(s => new Point() { X = (int)s.X, Y = (int)s.Y }).ToList();
        }

        public void SetPercentPoints(List<Point> points)
        {
            percentPoints = points.Select(s => new PointF(s.X, s.Y)).ToArray();
        }
    }
}
