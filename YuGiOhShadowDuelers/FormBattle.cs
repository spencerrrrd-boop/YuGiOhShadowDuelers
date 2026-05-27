using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YuGiOhShadowDuelers
{
    public partial class FormBattle : Form
    {
        // ── Engine ────────────────────────────────────────────────────────────
        private readonly BattleEngine _engine;
        private readonly CardStats    _playerCard;
        private readonly string       _prefix;

        // ── Timers ────────────────────────────────────────────────────────────
        private readonly System.Windows.Forms.Timer _turnTimer = new() { Interval = 1500 };
        private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 50 };
        private float _phase = 0f;

        // ── State ─────────────────────────────────────────────────────────────
        private string _playerState = "idle";
        private string _kaibaState  = "idle";
        private string _lastMessage = "¡Presiona INICIAR BATALLA para comenzar el duelo!";
        private bool   _gameOver    = false;

        // ── Log ───────────────────────────────────────────────────────────────
        private readonly List<string> _log = new();

        // ── Off-screen canvas ─────────────────────────────────────────────────
        private Bitmap   _canvas  = null!;
        private Graphics _gCanvas = null!;
        private Image    _bg;

        // ── Layout ────────────────────────────────────────────────────────────
        private const int SPRITE_W = 200;
        private const int SPRITE_H = 250;
        private const int PLAYER_X = 100;
        private const int KAIBA_X  = 1050;
        private const int SPRITE_Y = 260;
        private const int HP_BAR_Y = 530;
        private const int HP_BAR_W = 340;
        private const int HP_BAR_H = 30;

        public FormBattle(CardStats playerCard)
        {
            _playerCard = playerCard;
            _prefix     = playerCard.SpritePrefix;
            _engine     = new BattleEngine(playerCard);
            _bg         = SpriteManager.GetBackground("battle_arena");

            InitializeComponent();
            SetupForm();
            SetupButtons();

            _turnTimer.Tick += TurnTimer_Tick;
            _animTimer.Tick += (s, e) => { _phase += 0.07f; Invalidate(); };
            _animTimer.Start();
        }

        private void SetupForm()
        {
            Text            = "Yu-Gi-Oh! Shadow Duelers — ¡Batalla!";
            ClientSize      = new Size(1366, 768);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.Black;

            // Double buffer
            SetStyle(ControlStyles.AllPaintingInWmPaint  |
                     ControlStyles.UserPaint             |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            _canvas  = new Bitmap(ClientSize.Width, ClientSize.Height);
            _gCanvas = Graphics.FromImage(_canvas);
            _gCanvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            _gCanvas.SmoothingMode     = SmoothingMode.AntiAlias;
        }

        private void SetupButtons()
        {
            var btnStart = new Button
            {
                Name      = "btnStart",
                Text      = "⚔  INICIAR BATALLA",
                Size      = new Size(280, 50),
                Location  = new Point(ClientSize.Width / 2 - 140, 700),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Impact", 16f, FontStyle.Bold),
                ForeColor = Color.Gold,
                BackColor = Color.FromArgb(180, 20, 10, 0),
                Cursor    = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderColor = Color.Gold;
            btnStart.FlatAppearance.BorderSize  = 2;
            btnStart.Click += (s, e) =>
            {
                btnStart.Visible = false;
                _lastMessage     = "¡El duelo comienza!";
                _turnTimer.Start();
            };

            var btnRestart = new Button
            {
                Name      = "btnRestart",
                Text      = "🔄  JUGAR DE NUEVO",
                Size      = new Size(260, 50),
                Location  = new Point(ClientSize.Width / 2 - 280, 700),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Impact", 14f, FontStyle.Bold),
                ForeColor = Color.LightGreen,
                BackColor = Color.FromArgb(180, 0, 30, 0),
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            btnRestart.FlatAppearance.BorderColor = Color.LightGreen;
            btnRestart.FlatAppearance.BorderSize  = 2;
            btnRestart.Click += (s, e) =>
            {
                _animTimer.Stop();
                Hide();
                var sel = new FormCardSelect();
                sel.FormClosed += (ss, ee) => Close();
                sel.Show();
            };

            var btnExit = new Button
            {
                Name      = "btnExit",
                Text      = "🚪  SALIR",
                Size      = new Size(200, 50),
                Location  = new Point(ClientSize.Width / 2 + 60, 700),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Impact", 14f, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                BackColor = Color.FromArgb(180, 30, 0, 0),
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            btnExit.FlatAppearance.BorderColor = Color.OrangeRed;
            btnExit.FlatAppearance.BorderSize  = 2;
            btnExit.Click += (s, e) => Application.Exit();

            Controls.AddRange(new Control[] { btnStart, btnRestart, btnExit });
        }

        // ── Turn logic ────────────────────────────────────────────────────────
        private void TurnTimer_Tick(object? sender, EventArgs e)
        {
            if (_engine.IsGameOver) { EndGame(); return; }

            var (pLog, kLog) = _engine.ExecuteTurn();

            string pMsg = BuildMessage(pLog, isPlayerAttacking: true);
            _log.Insert(0, $"[T{pLog.TurnNumber}] {pMsg}");
            SetSprite(ref _playerState, pLog, attacker: true);
            SetSprite(ref _kaibaState,  pLog, attacker: false);

            if (kLog != null)
            {
                string kMsg = BuildMessage(kLog, isPlayerAttacking: false);
                _log.Insert(0, $"      ↩ {kMsg}");
                SetSprite(ref _kaibaState,  kLog, attacker: true);
                SetSprite(ref _playerState, kLog, attacker: false);
            }

            if (_log.Count > 9) _log.RemoveRange(9, _log.Count - 9);
            _lastMessage = pMsg;
            Invalidate();

            if (_engine.IsGameOver) EndGame();
        }

        private static string BuildMessage(TurnLog log, bool isPlayerAttacking)
        {
            string atk = isPlayerAttacking ? "FARAÓN" : "KAIBA";
            string def = isPlayerAttacking ? "KAIBA"  : "FARAÓN";

            if (log.RawDamage == 0 && !log.WasDodged)
                return $"{atk} se recupera del mareo (turno perdido)";
            if (log.WasDodged)
                return $"¡{def} ESQUIVÓ el ataque de {atk}!";
            if (log.WasMaxDamage)
                return $"⚡ {atk} GOLPE MÁXIMO {log.RawDamage} dmg! " +
                       $"{def} aturdido | regen 10% → neto {log.NetDamage}";
            return $"{atk} ataca por {log.RawDamage} dmg " +
                   $"(regen {log.RawDamage - log.NetDamage}) = {log.NetDamage} neto";
        }

        private static void SetSprite(ref string state, TurnLog log, bool attacker)
        {
            if (attacker)
                state = (log.RawDamage == 0 || log.WasDodged) ? "idle" : "attack";
            else
            {
                if (log.WasDodged)         state = "idle";
                else if (log.WasMaxDamage) state = "stunned";
                else if (log.NetDamage > 0) state = "hit";
                else                       state = "idle";
            }
        }

        private void EndGame()
        {
            _turnTimer.Stop();
            _gameOver    = true;
            bool won     = _engine.PlayerWon;
            _playerState = won ? "victory" : "defeat";
            _kaibaState  = won ? "defeat"  : "victory";
            _lastMessage = won
                ? "🏆 ¡EL FARAÓN GANA! ¡Es hora de duelo!"
                : "💀 ¡KAIBA GANA! El poder del Dragón de Ojos Azules triunfa.";

            Controls["btnRestart"]!.Visible = true;
            Controls["btnExit"]!.Visible    = true;
            Invalidate();
        }

        // ── Painting ──────────────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = _gCanvas;

            // Background
            g.DrawImage(_bg, 0, 0, ClientSize.Width, ClientSize.Height);
            using var ov = new SolidBrush(Color.FromArgb(60, 0, 0, 10));
            g.FillRectangle(ov, ClientRectangle);

            DrawTurnCounter(g);
            DrawSprites(g);
            DrawHPBars(g);
            DrawMessageBox(g);
            DrawLog(g);
            if (_gameOver) DrawGameOverBanner(g);

            // Blit
            e.Graphics.DrawImageUnscaled(_canvas, 0, 0);
        }

        private void DrawTurnCounter(Graphics g)
        {
            using var f  = new Font("Impact", 22f, FontStyle.Bold);
            string text  = _engine.IsGameOver ? "FIN DEL DUELO"
                                              : $"TURNO  {_engine.Turn}";
            int cx = ClientSize.Width / 2;
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(text, f, Brushes.Black, cx + 2, 11, sf);
            g.DrawString(text, f, Brushes.Gold,  cx,      9,  sf);
        }

        private void DrawSprites(Graphics g)
        {
            // Player
            var pImg = SpriteManager.Get(SpriteManager.PlayerSprite(_prefix, _playerState));
            g.DrawImage(pImg, PLAYER_X, SPRITE_Y, SPRITE_W, SPRITE_H);

            using var nf = new Font("Impact", 13f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(_playerCard.Name.ToUpper(), nf, Brushes.Black,
                         PLAYER_X + SPRITE_W / 2 + 2, SPRITE_Y + SPRITE_H + 6, sf);
            g.DrawString(_playerCard.Name.ToUpper(), nf, Brushes.Gold,
                         PLAYER_X + SPRITE_W / 2,     SPRITE_Y + SPRITE_H + 4, sf);

            // Kaiba (flipped)
            var kImg = SpriteManager.Get(SpriteManager.KaibaSprite(_kaibaState));
            var kBmp = new Bitmap(kImg);
            kBmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            g.DrawImage(kBmp, KAIBA_X, SPRITE_Y, SPRITE_W, SPRITE_H);
            kBmp.Dispose();

            g.DrawString("SETO KAIBA", nf, Brushes.Black,
                         KAIBA_X + SPRITE_W / 2 + 2, SPRITE_Y + SPRITE_H + 6, sf);
            g.DrawString("SETO KAIBA", nf, new SolidBrush(Color.CornflowerBlue),
                         KAIBA_X + SPRITE_W / 2,     SPRITE_Y + SPRITE_H + 4, sf);
        }

        private void DrawHPBars(Graphics g)
        {
            DrawOneHPBar(g, _engine.Player, new Point(PLAYER_X, HP_BAR_Y));
            DrawOneHPBar(g, _engine.Kaiba,  new Point(KAIBA_X,  HP_BAR_Y));
        }

        private static void DrawOneHPBar(Graphics g, Fighter f, Point pos)
        {
            // Background
            using var bg = new SolidBrush(Color.FromArgb(160, 20, 0, 0));
            g.FillRectangle(bg, pos.X, pos.Y, HP_BAR_W, HP_BAR_H);

            // Fill
            float ratio = Math.Max(0, (float)f.CurrentHP / f.MaxHP);
            int   fillW = (int)(HP_BAR_W * ratio);
            Color fill  = ratio > 0.5f ? Color.LimeGreen
                        : ratio > 0.25f ? Color.Yellow
                        : Color.OrangeRed;
            using var fb = new SolidBrush(fill);
            g.FillRectangle(fb, pos.X, pos.Y, fillW, HP_BAR_H);

            // Border
            g.DrawRectangle(new Pen(Color.Gold, 2), pos.X, pos.Y, HP_BAR_W, HP_BAR_H);

            // Text
            using var hf  = new Font("Consolas", 11f, FontStyle.Bold);
            string hpText = $"HP: {f.CurrentHP} / {f.MaxHP}";
            g.DrawString(hpText, hf, Brushes.White,
                         new RectangleF(pos.X + 5, pos.Y + 6, HP_BAR_W - 10, HP_BAR_H));
        }

        private void DrawMessageBox(Graphics g)
        {
            var rect = new Rectangle(ClientSize.Width / 2 - 380, 620, 760, 62);
            using var boxBg = new SolidBrush(Color.FromArgb(190, 0, 0, 30));
            g.FillRectangle(boxBg, rect);
            g.DrawRectangle(new Pen(Color.FromArgb(200, Color.Gold), 2), rect);

            using var mf = new Font("Consolas", 12f, FontStyle.Bold);
            var sf = new StringFormat { Alignment     = StringAlignment.Center,
                                        LineAlignment = StringAlignment.Center };
            g.DrawString(_lastMessage, mf, Brushes.White, rect, sf);
        }

        private void DrawLog(Graphics g)
        {
            int lx = 10, ly = 45;
            using var lf = new Font("Consolas", 9f);
            for (int i = 0; i < Math.Min(_log.Count, 9); i++)
            {
                int a = Math.Max(40, 240 - i * 25);
                using var br = new SolidBrush(Color.FromArgb(a, Color.LightCyan));
                g.DrawString(_log[i], lf, br, lx, ly + i * 15);
            }
        }

        private void DrawGameOverBanner(Graphics g)
        {
            bool win  = _engine.PlayerWon;
            string txt = win ? "★  ¡VICTORIA!" : "✖  DERROTA";
            Color  col = win ? Color.Gold       : Color.OrangeRed;

            float glow = (float)(Math.Sin(_phase * 2) * 0.5 + 0.5);
            int sz = (int)(44 + glow * 6);
            using var f  = new Font("Impact", sz, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            int cx = ClientSize.Width / 2;
            g.DrawString(txt, f, new SolidBrush(Color.FromArgb(180, Color.Black)), cx + 2, 152, sf);
            g.DrawString(txt, f, new SolidBrush(col), cx, 150, sf);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            ResumeLayout(false);
        }
    }

    internal static class TurnLogExt
    {
        public static bool SkipFlag(this TurnLog log) =>
            log.RawDamage == 0 && !log.WasDodged && log.Result != TurnResult.GameOver;
    }
}
