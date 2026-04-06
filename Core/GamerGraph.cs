using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ApexGamerBooster.Controls
{
    public class GamerGraph : Control
    {
        private Queue<float> dataPoints = new Queue<float>();
        private int maxPoints = 60;
        private Color graphColor = Color.FromArgb(0, 255, 136);
        private Color glowColor = Color.FromArgb(0, 255, 136, 40);
        private Color gridColor = Color.FromArgb(30, 30, 50);
        private string label = "";
        private string unit = "%";
        private float currentValue = 0;

        public Color GraphColor
        {
            get => graphColor;
            set
            {
                graphColor = value;
                glowColor = Color.FromArgb(value.R, value.G, value.B, 40);
                Invalidate();
            }
        }

        public string Label
        {
            get => label;
            set { label = value; Invalidate(); }
        }

        public string Unit
        {
            get => unit;
            set { unit = value; Invalidate(); }
        }

        public int MaxPoints
        {
            get => maxPoints;
            set { maxPoints = value; TrimData(); Invalidate(); }
        }

        public GamerGraph()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            Font = new Font("Consolas", 9f);
        }

        public void PushValue(float value)
        {
            currentValue = value;
            dataPoints.Enqueue(value);
            TrimData();
            Invalidate();
        }

        private void TrimData()
        {
            while (dataPoints.Count > maxPoints)
                dataPoints.Dequeue();
        }

        public void ClearData()
        {
            dataPoints.Clear();
            currentValue = 0;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = Width;
            int h = Height;

            using (var bg = new SolidBrush(Color.FromArgb(12, 12, 20)))
            {
                g.FillRectangle(bg, 0, 0, w, h);
            }

            for (int i = 0; i <= 4; i++)
            {
                float y = (h * i) / 4f;
                using (var pen = new Pen(gridColor, 1f))
                {
                    g.DrawLine(pen, 0, y, w, y);
                }
                float val = 100 - (i * 25);
                using (var brush = new SolidBrush(Color.FromArgb(80, 80, 100)))
                {
                    g.DrawString($"{val}{unit}", Font, brush, 2, y + 1);
                }
            }

            if (dataPoints.Count > 1)
            {
                var points = dataPoints.ToArray();
                float stepX = (float)w / (maxPoints - 1);
                float startX = w - (points.Length * stepX);

                using (var fillPath = new GraphicsPath())
                {
                    float firstY = h - (points[0] / 100f) * h;
                    fillPath.AddLine(startX, h, startX, firstY);
                    for (int i = 1; i < points.Length; i++)
                    {
                        float x = startX + i * stepX;
                        float y = h - (points[i] / 100f) * h;
                        float prevX = startX + (i - 1) * stepX;
                        float prevY = h - (points[i - 1] / 100f) * h;
                        float cpX = (prevX + x) / 2f;
                        fillPath.AddBezier(prevX, prevY, cpX, prevY, cpX, y, x, y);
                    }
                    fillPath.AddLine(startX + (points.Length - 1) * stepX, h, startX, h);

                    using (var fillBrush = new LinearGradientBrush(
                        new Rectangle(0, 0, 1, h),
                        Color.FromArgb(graphColor.R, graphColor.G, graphColor.B, 50),
                        Color.FromArgb(graphColor.R, graphColor.G, graphColor.B, 5),
                        LinearGradientMode.Vertical))
                    {
                        g.FillPath(fillBrush, fillPath);
                    }
                }

                using (var linePath = new GraphicsPath())
                {
                    float firstY = h - (points[0] / 100f) * h;
                    linePath.AddLine(startX, firstY, startX, firstY);
                    for (int i = 1; i < points.Length; i++)
                    {
                        float x = startX + i * stepX;
                        float y = h - (points[i] / 100f) * h;
                        float prevX = startX + (i - 1) * stepX;
                        float prevY = h - (points[i - 1] / 100f) * h;
                        float cpX = (prevX + x) / 2f;
                        linePath.AddBezier(prevX, prevY, cpX, prevY, cpX, y, x, y);
                    }

                    using (var glowPen = new Pen(glowColor, 4f))
                    {
                        g.DrawPath(glowPen, linePath);
                    }
                    using (var mainPen = new Pen(graphColor, 2f))
                    {
                        g.DrawPath(mainPen, linePath);
                    }
                }

                float lastX = startX + (points.Length - 1) * stepX;
                float lastY = h - (points[points.Length - 1] / 100f) * h;
                using (var glowBrush = new SolidBrush(Color.FromArgb(graphColor.R, graphColor.G, graphColor.B, 60)))
                {
                    g.FillEllipse(glowBrush, lastX - 6, lastY - 6, 12, 12);
                }
                using (var dotBrush = new SolidBrush(graphColor))
                {
                    g.FillEllipse(dotBrush, lastX - 3, lastY - 3, 6, 6);
                }
            }

            if (!string.IsNullOrEmpty(label))
            {
                using (var labelBrush = new SolidBrush(Color.FromArgb(180, 180, 200)))
                {
                    g.DrawString(label, Font, labelBrush, 2, 2);
                }
            }

            using (var valBrush = new SolidBrush(graphColor))
            {
                string valText = $"{currentValue:F1}{unit}";
                var size = g.MeasureString(valText, Font);
                g.DrawString(valText, Font, valBrush, w - size.Width - 4, 2);
            }
        }
    }
}