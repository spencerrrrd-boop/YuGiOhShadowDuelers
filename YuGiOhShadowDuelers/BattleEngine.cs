using System;

namespace YuGiOhShadowDuelers
{
    public enum TurnResult { Normal, MaxDamageHit, Dodged, GameOver }

    public class Fighter
    {
        public string    Name         { get; }
        public int       MaxHP        { get; }
        public int       CurrentHP    { get; private set; }
        public int       DamageMin    { get; }
        public int       DamageMax    { get; }
        public double    DodgeChance  { get; }
        public bool      SkipNextTurn { get; set; }

        private readonly Random _rng;

        public Fighter(CardStats stats, Random rng)
        {
            Name        = stats.Name;
            MaxHP       = stats.MaxHP;
            CurrentHP   = stats.MaxHP;
            DamageMin   = stats.DamageMin;
            DamageMax   = stats.DamageMax;
            DodgeChance = stats.DodgeChance;
            _rng        = rng;
        }

        public bool IsAlive => CurrentHP > 0;

        public int RollDamage() =>
            _rng.Next(DamageMin, DamageMax + 1);

        public bool RollDodge() =>
            _rng.NextDouble() < DodgeChance;

        /// <summary>Applies damage and regeneration rules. Returns actual damage dealt.</summary>
        public int TakeDamage(int rawDamage, bool isMaxDamage)
        {
            if (isMaxDamage)
            {
                // Rule 3: if max damage → skip next turn + regen 10% of damage received
                SkipNextTurn = true;
                int regen = (int)(rawDamage * 0.10);
                CurrentHP = Math.Max(0, CurrentHP - rawDamage + regen);
                return rawDamage - regen;   // net damage shown in log
            }
            else
            {
                // Rule 5: recover 5% of damage received each turn
                int regen = (int)(rawDamage * 0.05);
                CurrentHP = Math.Max(0, CurrentHP - rawDamage + regen);
                return rawDamage - regen;
            }
        }

        public void ClampHP() =>
            CurrentHP = Math.Clamp(CurrentHP, 0, MaxHP);
    }

    public class TurnLog
    {
        public int    TurnNumber   { get; set; }
        public string AttackerName { get; set; } = "";
        public string DefenderName { get; set; } = "";
        public int    RawDamage    { get; set; }
        public int    NetDamage    { get; set; }
        public bool   WasDodged    { get; set; }
        public bool   WasMaxDamage { get; set; }
        public int    AttackerHP   { get; set; }
        public int    DefenderHP   { get; set; }
        public TurnResult Result   { get; set; }
    }

    public class BattleEngine
    {
        private readonly Fighter _player;
        private readonly Fighter _kaiba;
        private readonly Random  _rng = new();
        private int _turn = 0;

        public Fighter Player => _player;
        public Fighter Kaiba  => _kaiba;
        public int     Turn   => _turn;

        public BattleEngine(CardStats playerCard)
        {
            _player = new Fighter(playerCard,      _rng);
            _kaiba  = new Fighter(CardStats.Kaiba, _rng);
        }

        /// <summary>
        /// Executes one full turn (player attacks Kaiba, then Kaiba attacks player).
        /// Returns two logs: [0]=player→kaiba, [1]=kaiba→player (null if game over after first).
        /// </summary>
        public (TurnLog playerLog, TurnLog? kaibaLog) ExecuteTurn()
        {
            _turn++;

            // ── PLAYER attacks KAIBA ──────────────────────────────────────────
            TurnLog playerLog = new() { TurnNumber = _turn,
                AttackerName = _player.Name, DefenderName = _kaiba.Name };

            if (_player.SkipNextTurn)
            {
                _player.SkipNextTurn = false;
                playerLog.Result     = TurnResult.Normal;
                playerLog.WasDodged  = false;
                playerLog.RawDamage  = 0;
                playerLog.NetDamage  = 0;
            }
            else if (_kaiba.RollDodge())
            {
                playerLog.WasDodged = true;
                playerLog.Result    = TurnResult.Dodged;
            }
            else
            {
                int dmg          = _player.RollDamage();
                bool isMax       = (dmg == _player.DamageMax);
                int net          = _kaiba.TakeDamage(dmg, isMax);
                playerLog.RawDamage    = dmg;
                playerLog.NetDamage    = net;
                playerLog.WasMaxDamage = isMax;
                playerLog.Result       = isMax ? TurnResult.MaxDamageHit : TurnResult.Normal;
            }

            playerLog.AttackerHP = _player.CurrentHP;
            playerLog.DefenderHP = _kaiba.CurrentHP;

            if (!_kaiba.IsAlive)
            {
                playerLog.Result = TurnResult.GameOver;
                return (playerLog, null);
            }

            // ── KAIBA attacks PLAYER ──────────────────────────────────────────
            TurnLog kaibaLog = new() { TurnNumber = _turn,
                AttackerName = _kaiba.Name, DefenderName = _player.Name };

            if (_kaiba.SkipNextTurn)
            {
                _kaiba.SkipNextTurn = false;
                kaibaLog.Result    = TurnResult.Normal;
                kaibaLog.RawDamage = 0;
                kaibaLog.NetDamage = 0;
            }
            else if (_player.RollDodge())
            {
                kaibaLog.WasDodged = true;
                kaibaLog.Result    = TurnResult.Dodged;
            }
            else
            {
                int dmg          = _kaiba.RollDamage();
                bool isMax       = (dmg == _kaiba.DamageMax);
                int net          = _player.TakeDamage(dmg, isMax);
                kaibaLog.RawDamage    = dmg;
                kaibaLog.NetDamage    = net;
                kaibaLog.WasMaxDamage = isMax;
                kaibaLog.Result       = isMax ? TurnResult.MaxDamageHit : TurnResult.Normal;
            }

            kaibaLog.AttackerHP = _kaiba.CurrentHP;
            kaibaLog.DefenderHP = _player.CurrentHP;

            if (!_player.IsAlive)
                kaibaLog.Result = TurnResult.GameOver;

            return (playerLog, kaibaLog);
        }

        public bool IsGameOver => !_player.IsAlive || !_kaiba.IsAlive;
        public bool PlayerWon  => _kaiba.CurrentHP <= 0 && _player.CurrentHP > 0;
    }
}
