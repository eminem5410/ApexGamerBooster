using System;
using System.Drawing;
using System.Windows.Forms;

namespace ApexGamerBooster.Utils
{
    public static class UIHelper
    {
        public static readonly Color BgPrimary = Color.FromArgb(10, 10, 15);
        public static readonly Color BgSecondary = Color.FromArgb(18, 18, 28);
        public static readonly Color BgTertiary = Color.FromArgb(25, 25, 40);
        public static readonly Color AccentGreen = Color.FromArgb(0, 255, 136);
        public static readonly Color AccentRed = Color.FromArgb(255, 51, 102);
        public static readonly Color AccentBlue = Color.FromArgb(0, 170, 255);
        public static readonly Color AccentYellow = Color.FromArgb(255, 204, 0);
        public static readonly Color AccentPurple = Color.FromArgb(170, 100, 255);
        public static readonly Color TextPrimary = Color.FromArgb(220, 220, 240);
        public static readonly Color TextSecondary = Color.FromArgb(120, 120, 150);
        public static readonly Color BorderColor = Color.FromArgb(40, 40, 60);

        public static void FadeIn(Form form, int duration)
        {
            form.Opacity = 0;
            var timer = new Timer { Interval = 15 };
            timer.Tick += (s, e) =>
            {
                form.Opacity += 0.05;
                if (form.Opacity >= 1.0)
                {
                    form.Opacity = 1.0;
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        public static Color GetStatusColor(float value)
        {
            if (value < 50) return AccentGreen;
            if (value < 80) return AccentYellow;
            return AccentRed;
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F1") + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024.0 * 1024.0)).ToString("F1") + " MB";
            return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("F2") + " GB";
        }

        public static TextBox CreateLogTextBox()
        {
            return new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BackColor = BgTertiary,
                ForeColor = TextSecondary,
                Font = new Font("Consolas", 8f),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
            };
        }

        public static Panel CreateCard(int x, int y, int width, int height, Color accentColor)
        {
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(18, 18, 30)
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.DrawRectangle(new Pen(Color.FromArgb(40, 40, 60), 1f), 0, 0, panel.Width - 1, panel.Height - 1);
                using (var pen = new Pen(Color.FromArgb(accentColor.R, accentColor.G, accentColor.B, 100), 2f))
                {
                    g.DrawLine(pen, 0, 0, panel.Width, 0);
                }
            };
            return panel;
        }
    }
}