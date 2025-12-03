// Create new file: D:\BillingSoftware\Forms\LoadingOverlay.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BillingSoftware.Forms
{
    public class LoadingOverlay : Form
    {
        private Label loadingLabel;
        private ProgressBar progressBar;

        public LoadingOverlay(string message = "Loading...")
        {
            this.Text = "";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(50, 50, 50, 50);
            this.Size = new Size(300, 150);
            this.Opacity = 0.9;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            Panel contentPanel = new Panel();
            contentPanel.Size = new Size(250, 100);
            contentPanel.Location = new Point(25, 25);
            contentPanel.BackColor = Color.White;
            contentPanel.BorderStyle = BorderStyle.None;

            loadingLabel = new Label();
            loadingLabel.Text = message;
            loadingLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            loadingLabel.ForeColor = Color.FromArgb(44, 62, 80);
            loadingLabel.Location = new Point(20, 20);
            loadingLabel.Size = new Size(210, 30);
            loadingLabel.TextAlign = ContentAlignment.MiddleCenter;

            progressBar = new ProgressBar();
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Location = new Point(20, 60);
            progressBar.Size = new Size(210, 20);
            progressBar.MarqueeAnimationSpeed = 30;

            contentPanel.Controls.Add(loadingLabel);
            contentPanel.Controls.Add(progressBar);
            this.Controls.Add(contentPanel);
        }
    }
}