using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace YuGiOhShadowDuelers
{
    public partial class FormCardSelect : Form
    {
        private readonly (CardType type, string title, string imgName, Color accent)[] _cards =
        {
            (CardType.SliferTheSkyDragon, "SLIFER EL DRAGÓN\nDEL CIELO", "slifer_idle",  Color.OrangeRed),
            (CardType.WingedDragonOfRa,   "EL DRAGÓN\nALADO DE RA",      "ra_idle",      Color.Gold),
            (CardType.DarkMagician,       "MAGO\nOSCURO",                 "dm_idle",      Color.MediumPurple),
            (CardType.DarkMagicianGirl,   "MAGA\nOSCURA",                 "dmg_idle",     Color.DeepPink),
        };

        private int    _selected = 0;
        private Image  _bg;
        private System.Windows.Forms.Timer _animTimer = null!;
        private float  _phase = 0f;

        // Off-screen canvas
        private Bitmap   _canvas  = null!;
        private Graphics _gCanvas = null!;

        private const int CARD_W = 200;
        private const int CARD_H = 260;
        private const int CARD_Y = 180;

        public FormCardSelect()
        {
            InitializeComponent();
            SetupForm();
            SetupButtons();
            _animTimer      = new System.Windows.Forms.Timer { Interval = 50 };
            _animTimer.Tick += (s, e) => { _phase += 0.06f; Invalidate(); };
            _animTimer.Start();
        }

        private void SetupForm()
        {
            Text            = "Elige tu Carta";
            ClientSize      = new Size(1366, 768);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.Black;

            SetStyle(ControlStyles.AllPaintingInWmPaint  |
                     ControlStyles.UserPaint             |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            _bg      = SpriteManager.GetBackground("card_select");
            _canvas  = new Bitmap(ClientSize.Width, ClientSize.Height);
            _gCanvas = Graphics.FromImage(_canvas);
            _gCanvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            _gCanvas.SmoothingMode     = SmoothingMode.AntiAlias;
        }

        private void SetupButtons()
        {
            var btnConfirm = new Button
            {
                Text      = "⚔  CONFIRMAR SELECCIÓN",
                Size      = new Size(340, 55),
                Location  = new Point((ClientSize.Width - 340) / 2, 680),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Impact", 18f, FontStyle.Bold),
                ForeColor = Color.Gold,
                BackColor = Color.FromArgb(180, 10, 5, 30),
                Cursor    = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderColor = Color.Gold;
            btnConfirm.FlatAppearance.BorderSize  = 2;
            btnConfirm.Click += (s, e) => StartBattle();
            Controls.Add(btnConfirm);
        }

        private int CardCenterX(int i)
        {
            int totalW = _cards.Length * (CARD_W + 30) - 30;
            int startX = (ClientSize.Width - totalW) / 2;
            return startX + i * (CARD_W + 30) + CARD_W / 2;
        }

        private Rectangle CardRect(int i) =>
            new(CardCenterX(i) - CARD_W / 2, CARD_Y, CARD_W, CARD_H);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = _gCanvas;

            // Background
            g.DrawImage(_bg, 0, 0, ClientSize.Width, ClientSize.Height);
            using var ov = new SolidBrush(Color.FromArgb(100, 0, 0, 20));
            g.FillRectangle(ov, ClientRectangle);

            // Title
            DrawShadowText(g, "── ELIGE TU CARTA ──",
                new Font("Impact", 28f, FontStyle.Bold),
                Color.Gold, Color.FromArgb(120, Color.DarkGoldenrod),
                new Point(ClientSize.Width / 2, 80), centered: true);

            DrawShadowText(g, "← → para navegar  |  ENTER para confirmar",
                new Font("Impact", 13f),
                Color.FromArgb(200, Color.Violet), Color.Black,
                new Point(ClientSize.Width / 2, 740), centered: true);

            for (int i = 0; i < _cards.Length; i++) DrawCard(g, i);
            DrawStatsPanel(g);

            e.Graphics.DrawImageUnscaled(_canvas, 0, 0);
        }

        private void DrawCard(Graphics g, int i)
        {
            var rect   = CardRect(i);
            var card   = _cards[i];
            bool isSel = (i == _selected);

            if (isSel)
            {
                float glow = (float)(Math.Sin(_phase) * 0.5 + 0.5);
                int radius = (int)(10 + glow * 8);
                for (int r = radius; r > 0; r -= 2)
                {
                    using var gPen = new Pen(Color.FromArgb((int)(60 * r / radius), card.accent), r * 2);
                    g.DrawRectangle(gPen, rect.X - r, rect.Y - r,
                                    rect.Width + r * 2, rect.Height + r * 2);
                }
            }

            using var cardBg = new SolidBrush(isSel
                ? Color.FromArgb(200, 10, 5, 40)
                : Color.FromArgb(150, 5, 5, 20));
            g.FillRectangle(cardBg, rect);

            using var borderPen = new Pen(isSel ? card.accent
                : Color.FromArgb(80, Color.Gray), isSel ? 3 : 1);
            g.DrawRectangle(borderPen, rect);

            var sprite = SpriteManager.Get(card.imgName);
            g.DrawImage(sprite, new Rectangle(rect.X + 20, rect.Y + 10, CARD_W - 40, 160));

            using var titleFont  = new Font("Impact", 11f, FontStyle.Bold);
            using var titleBrush = new SolidBrush(isSel ? card.accent : Color.LightGray);
            var sf = new StringFormat { Alignment = StringAlignment.Center,
                                        LineAlignment = StringAlignment.Center };
            g.DrawString(card.title, titleFont, titleBrush,
                         new Rectangle(rect.X, rect.Y + 175, CARD_W, 80), sf);

            if (isSel)
            {
                float pulse = (float)(Math.Sin(_phase * 2) * 0.5 + 0.5);
                using var arrowBrush = new SolidBrush(
                    Color.FromArgb((int)(160 + pulse * 95), card.accent));
                g.FillPolygon(arrowBrush, new[]
                {
                    new Point(rect.X + CARD_W / 2,      rect.Y - 20),
                    new Point(rect.X + CARD_W / 2 - 12, rect.Y - 5),
                    new Point(rect.X + CARD_W / 2 + 12, rect.Y - 5),
                });
            }
        }

        private void DrawStatsPanel(Graphics g)
        {
            var stats  = CardStats.FromCardType(_cards[_selected].type);
            var accent = _cards[_selected].accent;
            var panel  = new Rectangle(ClientSize.Width / 2 - 220, 480, 440, 145);

            using var panelBg = new SolidBrush(Color.FromArgb(180, 5, 5, 30));
            g.FillRectangle(panelBg, panel);
            g.DrawRectangle(new Pen(accent, 2), panel);

            using var font   = new Font("Consolas", 11f, FontStyle.Bold);
            using var fBrush = new SolidBrush(Color.White);
            using var aBrush = new SolidBrush(accent);
            int x = panel.X + 16, y = panel.Y + 10;
            g.DrawString("ESTADÍSTICAS", new Font("Impact", 13f), aBrush, x, y); y += 28;
            g.DrawString($"♥  HP:       {stats.MaxHP}", font, fBrush, x, y); y += 22;
            g.DrawString($"⚔  DAÑO:    {stats.DamageMin} – {stats.DamageMax}", font, fBrush, x, y); y += 22;
            g.DrawString($"★  ESQUIVA: {stats.DodgeChance * 100:0}%", font, fBrush, x, y); y += 22;
            g.DrawString($"   {stats.Description}", new Font("Consolas", 9f), fBrush, x, y);
        }

        private static void DrawShadowText(Graphics g, string text, Font font,
            Color color, Color shadow, Point pos, bool centered = false)
        {
            var sf = centered ? new StringFormat { Alignment = StringAlignment.Center }
                              : StringFormat.GenericDefault;
            g.DrawString(text, font, new SolidBrush(shadow), pos.X + 2, pos.Y + 2, sf);
            g.DrawString(text, font, new SolidBrush(color),  pos.X,     pos.Y,     sf);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                    _selected = (_selected - 1 + _cards.Length) % _cards.Length;
                    Invalidate(); return true;
                case Keys.Right:
                    _selected = (_selected + 1) % _cards.Length;
                    Invalidate(); return true;
                case Keys.Enter:
                    StartBattle(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            for (int i = 0; i < _cards.Length; i++)
            {
                if (CardRect(i).Contains(e.Location))
                {
                    if (_selected == i) StartBattle();
                    else { _selected = i; Invalidate(); }
                    return;
                }
            }
        }

        private void StartBattle()
        {
            _animTimer.Stop();
            var card = CardStats.FromCardType(_cards[_selected].type);
            Hide();
            var battle = new FormBattle(card);
            battle.FormClosed += (s, e) => Close();
            battle.Show();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            ResumeLayout(false);
        }
    }
}
