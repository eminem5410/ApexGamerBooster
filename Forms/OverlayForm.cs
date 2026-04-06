using System;
using System.Drawing;
using System.Windows.Forms;
using ApexGamerBooster.Utils;

namespace ApexGamerBooster.Forms
{
    public class OverlayForm : Form
    {
        private Label lblCpu;
        private Label lblRam;
        private Label lblGpu;
        private Panel bgPanel;
        private bool isDragging;
        private Point dragStart;

        public OverlayForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            BackColor = Color.FromArgb(15, 15, 25);
            Opacity = 0.82;
            Size = new Size(195, 82);
            ShowInTaskbar = false;

            if (Screen.PrimaryScreen != null)
            {
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - 215, 8);
            }

            bgPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(12, 12, 22)
            };
            Controls.Add(bgPanel);

            lblCpu = CreateStatLabel("CPU: --.-%");
            lblCpu.Location = new Point(10, 8);
            bgPanel.Controls.Add(lblCpu);

            lblRam = CreateStatLabel("RAM: --.-%");
            lblRam.Location = new Point(10, 30);
            bgPanel.Controls.Add(lblRam);

            lblGpu = CreateStatLabel("GPU: --.-%");
            lblGpu.Location = new Point(10, 52);
            bgPanel.Controls.Add(lblGpu);

            bgPanel.MouseDown += BgPanel_MouseDown;
            bgPanel.MouseMove += BgPanel_MouseMove;
            bgPanel.MouseUp += BgPanel_MouseUp;

            Paint += OverlayForm_Paint;
        }

        private void BgPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStart = e.Location;
            }
        }

        private void BgPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int newX = Location.X + e.X - dragStart.X;
                int newY = Location.Y + e.Y - dragStart.Y;
                Location = new Point(newX, newY);
            }
        }

        private void BgPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(UIHelper.AccentGreen, 1f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private Label CreateStatLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = UIHelper.AccentGreen,
                AutoSize = true,
                BackColor = Color.Transparent
            };
        }

        public void UpdateValues(float cpu, float ram, float gpu)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateValues(cpu, ram, gpu)));
                return;
            }

            lblCpu.Text = $"CPU: {cpu:F1}%";
            lblCpu.ForeColor = UIHelper.GetStatusColor(cpu);

            lblRam.Text = $"RAM: {ram:F1}%";
            lblRam.ForeColor = UIHelper.GetStatusColor(ram);

            lblGpu.Text = $"GPU: {gpu:F1}%";
            lblGpu.ForeColor = UIHelper.GetStatusColor(gpu);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000080;
                return cp;
            }
        }
    }
}