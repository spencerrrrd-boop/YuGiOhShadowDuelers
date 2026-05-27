using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace YuGiOhShadowDuelers
{
    public partial class FormTitle : Form
    {
        private Image  _bg;
        private System.Windows.Forms.Timer _glowTimer = null!;
        private float  _glowPhase = 0f;
        private Button _btnStart  = null!;

        // Double-buffer canvas
        private Bitmap  _canvas   = null!;
        private Graphics _gCanvas = null!;

        public FormTitle()
        {
            InitializeComponent();
            SetupForm();
            SetupUI();
            SetupGlowAnimation();
        }

        private void SetupForm()
        {
            Text            = "Yu-Gi-Oh! Shadow Duelers";
            ClientSize      = new Size(1366, 768);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.Black;

            // ── Anti-flicker ──────────────────────────────────────────────────
            SetStyle(ControlStyles.AllPaintingInWmPaint  |
                     ControlStyles.UserPaint             |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            _bg     = SpriteManager.GetBackground("title_screen");
            _canvas = new Bitmap(ClientSize.Width, ClientSize.Height);
            _gCanvas = Graphics.FromImage(_canvas);
            _gCanvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        private void SetupUI()
        {
            _btnStart = new Button
            {
                Text      = "▶  INICIAR JUEGO",
                Size      = new Size(300, 60),
                Location  = new Point((ClientSize.Width - 300) / 2, 590),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Impact", 20f, FontStyle.Bold),
                ForeColor = Color.Gold,
                BackColor = Color.FromArgb(180, 20, 10, 0),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            _btnStart.FlatAppearance.BorderColor = Color.Gold;
            _btnStart.FlatAppearance.BorderSize  = 2;
            _btnStart.Click += BtnStart_Click;

            var lblEnter = new Label
            {
                Text      = "PRESIONA ENTER PARA COMENZAR",
                Font      = new Font("Impact", 13f),
                ForeColor = Color.FromArgb(200, Color.Violet),
                BackColor = Color.Transparent,
                AutoSize  = true,
            };
            lblEnter.Location = new Point((ClientSize.Width - lblEnter.PreferredWidth) / 2, 665);

            Controls.AddRange(new Control[] { _btnStart, lblEnter });
        }

        private void SetupGlowAnimation()
        {
            _glowTimer       = new System.Windows.Forms.Timer { Interval = 50 };
            _glowTimer.Tick += (s, e) => { _glowPhase += 0.08f; Invalidate(); };
            _glowTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw everything onto _canvas first, then blit once → no flicker
            var g = _gCanvas;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Background
            g.DrawImage(_bg, 0, 0, ClientSize.Width, ClientSize.Height);

            // Subtle dark overlay
            using var ov = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
            g.FillRectangle(ov, ClientRectangle);

            // Animated glow behind title
            float glow = (float)(Math.Sin(_glowPhase) * 0.5 + 0.5);
            int alpha  = (int)(30 + glow * 50);
            using var glowBrush = new SolidBrush(Color.FromArgb(alpha, Color.Gold));
            g.FillEllipse(glowBrush, ClientSize.Width / 2 - 300, 60, 600, 200);

            // Pulsing button color (update in-place, no repaint needed)
            float pulse = (float)(Math.Sin(_glowPhase * 1.5) * 0.5 + 0.5);
            _btnStart.ForeColor = Color.FromArgb((int)(160 + pulse * 95), Color.Gold);

            // Blit canvas to screen in one shot
            e.Graphics.DrawImageUnscaled(_canvas, 0, 0);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter) { OpenCardSelect(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BtnStart_Click(object? sender, EventArgs e) => OpenCardSelect();

        private void OpenCardSelect()
        {
            _glowTimer.Stop();
            Hide();
            var form = new FormCardSelect();
            form.FormClosed += (s, e) => Close();
            form.Show();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            ResumeLayout(false);
        }
    }
}
