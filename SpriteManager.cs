using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace YuGiOhShadowDuelers
{
    public static class SpriteManager
    {
        private static readonly Dictionary<string, Image> _cache = new();

        private static string AssetsPath =>
            Path.Combine(Application.StartupPath, "Resources");

        public static Image Get(string name)
        {
            if (_cache.TryGetValue(name, out var cached)) return cached;

            string path = Path.Combine(AssetsPath, "sprites", name + ".png");
            if (!File.Exists(path))
            {
                // Return a 160x200 placeholder magenta image so the game still runs
                var placeholder = new Bitmap(160, 200);
                using var g = Graphics.FromImage(placeholder);
                g.Clear(Color.FromArgb(80, Color.Magenta));
                g.DrawString(name, SystemFonts.DefaultFont, Brushes.White, 4, 90);
                _cache[name] = placeholder;
                return placeholder;
            }

            var img = Image.FromFile(path);
            _cache[name] = img;
            return img;
        }

        public static Image GetBackground(string name)
        {
            string key = "bg_" + name;
            if (_cache.TryGetValue(key, out var cached)) return cached;
            string path = Path.Combine(AssetsPath, "backgrounds", name + ".png");
            if (!File.Exists(path)) return new Bitmap(1366, 768);
            var img = Image.FromFile(path);
            _cache[key] = img;
            return img;
        }

        // ── Sprite name helpers ──────────────────────────────────────────────

        /// <summary>Returns the sprite name for a player card state.</summary>
        public static string PlayerSprite(string prefix, string state)
        {
            // state: idle | attack | victory | defeat | hit | stunned
            return state switch
            {
                "hit"     => "shared_hit",
                "stunned" => "shared_stunned",
                _         => $"{prefix}_{state}"
            };
        }

        public static string KaibaSprite(string state) => state switch
        {
            "idle"     => "kaiba_damaged_norm",   // kaiba standing
            "attack"   => "kaiba_attack_norm",
            "victory"  => "kaiba_victory_norm",
            "defeat"   => "kaiba_defeated_norm",
            "hit"      => "kaiba_damaged_norm",
            "stunned"  => "kaiba_damaged_norm",
            "special"  => "kaiba_special_charge_norm",
            "ultimate" => "kaiba_ultimate_norm",
            _          => "kaiba_damaged_norm"
        };
    }
}
