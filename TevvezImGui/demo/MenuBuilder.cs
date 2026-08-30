using System;
using System.Diagnostics;

namespace TevvezImGui.Demo
{
    /// <summary>
    /// Der Menueaufbau, einmal fuer beide Anwendungen: das eigene Fenster und
    /// das durchsichtige Overlay. Genau so baust du dein eigenes Menue.
    /// </summary>
    public static class MenuBuilder
    {
        public static Menu Build(Settings C, string cfgPath)
        {
            return new Menu("TEVVEZ CHEATS", "v1063", "Killing Floor  -  UE2  -  ImGui.NET")
            {
                new Page("Visuals", NavIcon.Eye, "ESP")
                {
                    new Card("World")
                    {
                        new Toggle("WallHack",    () => C.WallHack, "Gegner durch Waende sehen"),
                        new Toggle("No shadows",  () => C.NoShadows),
                        new Toggle("Remove fog",  () => C.RemoveFog),
                        new Toggle("Crosshair",   () => C.Crosshair),
                        new Toggle("Players ESP", () => C.PlayersEsp),
                    },

                    new Card("Zombie ESP")
                    {
                        new Toggle("Box",         () => C.BoxEsp),
                        new Toggle("3D box",      () => C.Box3D),
                        new Toggle("Skeleton",    () => C.Skeleton),
                        new Toggle("Health bar",  () => C.HealthBar),
                        new Toggle("Health text", () => C.HealthText),
                        new Toggle("Distance",    () => C.DistanceEsp),
                        new Toggle("Name",        () => C.NameEsp),
                    },

                    new Card("ESP style")
                    {
                        new Combo ("Box style",     () => C.BoxStyle, "Brackets", "Rectangle"),
                        new Combo ("Snaplines",     () => C.Snaplines, "Off", "From bottom", "From center", "From top"),
                        new Slider("Box thickness", () => C.BoxThickness, 1, 5, "0.0"),
                        new Slider("Text size",     () => C.TextSize, 0.7f, 1.8f, "0.00"),
                        new Slider("Max distance",  () => C.MaxDistance, 0, 300, "0"),
                        new Toggle("Distance fade", () => C.DistanceFade),
                    },

                    new Card("Radar")
                    {
                        new Toggle("Zombie radar",    () => C.Radar),
                        new Slider("Size",            () => C.RadarSize, 50, 300, "0"),
                        new Toggle("Simple style",    () => C.RadarSimple),
                        new Toggle("Square radar",    () => C.RadarSquare),
                        new Toggle("Sweep animation", () => C.RadarSweep),
                        new Slider("Background",      () => C.RadarBg, 0, 1, "0.00"),
                    },
                },

                new Page("Aimbot", NavIcon.Crosshair, "COMBAT")
                {
                    new Card("Aimbot")
                    {
                        new Toggle("Aimbot",      () => C.Aimbot),
                        new Combo ("Key",         () => C.AimKey, "Ctrl", "Shift", "Alt", "Right mouse", "Always on"),
                        new Toggle("Auto aimbot", () => C.AutoAimbot, "Schiesst selbst, sobald ein Ziel steht"),
                        new Toggle("Warning",     () => C.AimWarning),
                        new Toggle("Auto shot",   () => C.AutoShot),
                        new Slider("Shot delay",  () => C.AutoShotWait, 0, 1000, "0"),
                    },

                    new Card("Aim FOV")
                    {
                        new Toggle("Aim FOV",    () => C.AimFov),
                        new Slider("Size",       () => C.AimFovSize, 50, 800, "0"),
                        new Toggle("Smooth aim", () => C.SmoothAim, "Zieht weich nach statt sofort zu springen"),
                        new Slider("Speed",      () => C.SmoothSpeed, 10, 500, "0"),
                    },

                    new Card("Silent aim")
                    {
                        new Toggle("Silent aim",   () => C.SilentAim),
                        new Slider("Predict",      () => C.SilentLead, 0, 100, "0"),
                        new Toggle("Aim at body",  () => C.SilentBody),
                        new Toggle("Magic bullet", () => C.MagicBullet),
                        new Toggle("Aim lock",     () => C.AimLock),
                    },

                    new Card("Hitbox and weapon")
                    {
                        new Toggle("Big head hitbox", () => C.BigHead),
                        new Slider("Head size",       () => C.BigHeadSize, 100, 800, "0"),
                        new Toggle("No recoil",       () => C.NoRecoil),
                        new Toggle("No spread",       () => C.NoSpread),
                    },
                },

                new Page("Colors", NavIcon.Palette, "LOOK")
                {
                    new Card("ESP")
                    {
                        new ColorPick("Box - visible",     () => C.BoxVisible),
                        new ColorPick("Box - behind wall", () => C.BoxHidden),
                        new ColorPick("Skeleton",          () => C.SkeletonCol),
                        new ColorPick("Name",              () => C.NameCol),
                    },

                    new Card("Aim and radar")
                    {
                        new ColorPick("Aim FOV circle", () => C.FovCol),
                        new ColorPick("Crosshair",      () => C.CrosshairCol),
                        new ColorPick("Radar frame",    () => C.RadarFrame),
                    },

                    new Card("Accent")
                    {
                        new Note("Der Akzent faerbt Schalter, Regler und Kopfzeile."),
                        new Row(new Btn("Violett", () => SetAccent(0)),
                                new Btn("Pink",    () => SetAccent(3)),
                                new Btn("Blau",    () => SetAccent(4))),
                        new Row(new Btn("Gruen",   () => SetAccent(5)),
                                new Btn("Orange",  () => SetAccent(6))),
                    },
                },

                new Page("Settings", NavIcon.Gear, "SYSTEM")
                {
                    new Card("Watermark")
                    {
                        new Toggle("Show watermark", () => C.Watermark),
                        new Combo ("Position",       () => C.WatermarkPos, "Top left", "Top center", "Top right"),
                        new Toggle("Show FPS",       () => C.WatermarkFps),
                        new Toggle("Show zombies",   () => C.WatermarkZeds),
                        new Toggle("Show clock",     () => C.WatermarkTime),
                    },

                    new Card("Configuration")
                    {
                        new Note("Alle Werte stehen in " + cfgPath),
                        new Row(new Btn("Save now", () => ConfigStore.Save(C, cfgPath), true),
                                new Btn("Reload",   () => ConfigStore.Load(C, cfgPath)),
                                new Btn("Open",     () => { try { Process.Start(cfgPath); } catch { } })),
                    },
                },

                new Page("About", NavIcon.Info, "SYSTEM")
                {
                    new Card("Tevvez Cheats")
                    {
                        new Note("Ein fertiges Menue zum Kopieren: Seiten, Karten, Schalter, "
                               + "Regler, Auswahlfelder, Farbfelder und Knoepfe."),
                        new Line(),
                        new Note("Neue Seite:   new Page(\"Name\", NavIcon.Eye, \"GRUPPE\")"),
                        new Note("Neue Karte:   new Card(\"Titel\")"),
                        new Note("Schalter:     new Toggle(\"Text\", () => cfg.Feld)"),
                        new Note("Regler:       new Slider(\"Text\", () => cfg.Wert, 0, 100)"),
                        new Note("Auswahl:      new Combo(\"Text\", () => cfg.Index, \"A\", \"B\")"),
                        new Note("Farbe:        new ColorPick(\"Text\", () => cfg.Farbe)"),
                    },
                },
            }
            .Actions("Save config", () => ConfigStore.Save(C, cfgPath),
                     "Load config", () => ConfigStore.Load(C, cfgPath))
            .Status(() => new[] { DateTime.Now.ToString("HH:mm"), CountActive(C) + " on" });
        }

        private static void SetAccent(int index)
        {
            if (index >= 0 && index < Theme.AccentPresets.Length)
            {
                Theme.Accent = Theme.AccentPresets[index];
                Theme.Apply();   // Stil neu setzen, damit die Farbe ueberall greift
            }
        }

        private static int CountActive(Settings C)
        {
            int n = 0;
            foreach (var f in typeof(Settings).GetFields())
                if (f.FieldType == typeof(bool) && (bool)f.GetValue(C)) n++;
            return n;
        }
    }
}
