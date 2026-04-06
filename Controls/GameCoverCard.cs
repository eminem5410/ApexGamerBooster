using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ApexGamerBooster.Utils;

namespace ApexGamerBooster.Controls
{
    public class GameCoverCard : UserControl
    {
        private PictureBox picCover;
        private Label lblName;
        private Panel borderPanel;

        public string GamePath { get; set; }
        public string GameName { get; set; }

        public event Action<string> OnLaunchGame;
        public event Action<string> OnOptimizeGame;
        public event Action<string> OnOpenFiles;
        public event Action<string> OnUninstallGame;

        public GameCoverCard(string gameName, string exePath)
        {
            GameName = gameName;
            GamePath = exePath;

            Size = new Size(150, 200);
            Margin = new Padding(10);

            borderPanel = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.BorderColor };
            Controls.Add(borderPanel);

            var innerPanel = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.BgTertiary, Padding = new Padding(2) };
            borderPanel.Controls.Add(innerPanel);

            picCover = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.CenterImage, BackColor = UIHelper.BgSecondary };
            innerPanel.Controls.Add(picCover);

            lblName = new Label 
            { 
                Text = gameName.Length > 18 ? gameName.Substring(0, 15) + "..." : gameName,
                Dock = DockStyle.Bottom, 
                Height = 30, 
                BackColor = UIHelper.BgSecondary, 
                ForeColor = UIHelper.TextPrimary, 
                Font = new Font("Segoe UI", 8f, FontStyle.Bold), 
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 5, 0, 0)
            };
            innerPanel.Controls.Add(lblName);

            LoadGameImage(exePath);

            var contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = UIHelper.BgSecondary;
            contextMenu.ForeColor = UIHelper.TextPrimary;
            contextMenu.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColors());

            contextMenu.Items.Add("🚀 Lanzar con Boost", null, (s, e) => OnLaunchGame?.Invoke(GamePath));
            contextMenu.Items.Add("⚡ Optimizar", null, (s, e) => OnOptimizeGame?.Invoke(GamePath));
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("📁 Ver archivos locales", null, (s, e) => OnOpenFiles?.Invoke(GamePath));
            contextMenu.Items.Add("🗑️ Eliminar de la lista", null, (s, e) => OnUninstallGame?.Invoke(GamePath));

            ContextMenuStrip = contextMenu;
        }

        private void LoadGameImage(string exePath)
        {
            try
            {
                if (File.Exists(exePath))
                {
                    Icon exeIcon = Icon.ExtractAssociatedIcon(exePath);
                    if (exeIcon != null)
                    {
                        Bitmap bmp = new Bitmap(120, 120);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(exeIcon.ToBitmap(), 10, 10, 100, 100);
                        }
                        picCover.Image = bmp;
                    }
                }
                else
                {
                    picCover.Image = SystemIcons.Warning.ToBitmap();
                }
            }
            catch { picCover.Image = null; }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            borderPanel.BackColor = UIHelper.AccentGreen;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            borderPanel.BackColor = UIHelper.BorderColor;
        }
    }

    public class DarkMenuColors : ProfessionalColorTable
    {
        public override Color MenuBorder => UIHelper.BorderColor;
        public override Color MenuItemBorder => UIHelper.BorderColor;
        public override Color MenuItemSelected => UIHelper.BgTertiary;
        public override Color MenuStripGradientBegin => UIHelper.BgSecondary;
        public override Color MenuStripGradientEnd => UIHelper.BgSecondary;
        public override Color MenuItemPressedGradientBegin => UIHelper.AccentGreen;
        public override Color MenuItemPressedGradientEnd => UIHelper.AccentGreen;
    }
}