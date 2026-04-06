using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ApexGamerBooster.Controls
{
    public class RoundedButton : Button
    {
        private int borderRadius = 8;
        private Color borderColor = Color.FromArgb(0, 255, 136);
        private Color hoverColor = Color.FromArgb(0, 255, 136, 30);
        private bool isHovering = false;

        public int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public Color HoverColor
        {
            get => hoverColor;
            set { hoverColor = value; }
        }

        public bool Selected { get; set; } = false;

        public ContentAlignment TextAlignment { get; set; } = ContentAlignment.MiddleCenter;

        public RoundedButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.FromArgb(220, 220, 240);
            Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Cursor = Cursors.Hand;
            Size = new Size(120, 38);
            Margin = new Padding(0);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovering = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovering = false;
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = Width;
            int h = Height;
            int r = Math.Min(borderRadius, Math.Min(w / 2, h / 2));

            Color bgColor;
            if (Selected)
                bgColor = Color.FromArgb(30, 30, 50);
            else if (isHovering)
                bgColor = hoverColor;
            else
                bgColor = Color.FromArgb(18, 18, 30);

            Color currentBorder;
            if (Selected || isHovering)
                currentBorder = borderColor;
            else
                currentBorder = Color.FromArgb(borderColor.R, borderColor.G, borderColor.B, 60);

            Color textColor;
            if (Selected)
                textColor = borderColor;
            else if (isHovering)
                textColor = borderColor;
            else
                textColor = ForeColor;

            using (var path = CreateRoundedRectPath(0, 0, w, h, r))
            {
                using (var bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, path);
                }
                using (var borderPen = new Pen(currentBorder, Selected ? 2f : (isHovering ? 1.5f : 1f)))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            if (Selected)
            {
                using (var glowPen = new Pen(Color.FromArgb(borderColor.R, borderColor.G, borderColor.B, 40), 3f))
                {
                    g.DrawLine(glowPen, 0, 8, 0, h - 8);
                }
                using (var linePen = new Pen(borderColor, 2f))
                {
                    g.DrawLine(linePen, 0, 10, 0, h - 10);
                }
            }

            var textRect = new Rectangle(Padding.Left + 8, Padding.Top, w - Padding.Horizontal - 8, h - Padding.Vertical);

            TextFormatFlags flags = TextFormatFlags.WordEllipsis;
            switch (TextAlignment)
            {
                case ContentAlignment.MiddleLeft:
                    flags |= TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.MiddleRight:
                    flags |= TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
                    break;
                default:
                    flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                    break;
            }
            TextRenderer.DrawText(g, Text, Font, textRect, textColor, flags);
        }

        private GraphicsPath CreateRoundedRectPath(float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}