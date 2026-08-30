using System;
using System.Numerics;
using TevvezImGui.Demo;

namespace TevvezImGui.OverlayDemo
{
    /// <summary>
    /// Startet das Overlay mit demselben Menue wie das Fenster-Projekt und
    /// einer Testszene, damit man ESP, Radar, FOV und Watermark auch ohne
    /// Spiel sieht. Die Szene fuellst du spaeter mit deinen echten Daten.
    /// </summary>
    internal static class Program
    {
        private static readonly Settings     C     = new Settings();
        private static readonly OverlayScene Scene = new OverlayScene();
        private const string CfgPath = "tevvez.cfg";

        [STAThread]
        private static void Main(string[] args)
        {
            ConfigStore.Load(C, CfgPath);

            var m = MenuBuilder.Build(C, CfgPath);

            // Menue-Einstellungen auf Watermark und Szene uebertragen
            m.BeforeDraw = () =>
            {
                m.Watermark.Show     = C.Watermark;
                m.Watermark.Position = C.WatermarkPos;
                m.Watermark.ShowFps  = C.WatermarkFps;
                m.Watermark.ShowTime = C.WatermarkTime;
                m.Watermark.Extra    = C.WatermarkZeds
                                     ? (Func<string>)(() => Scene.Entities.Count + " zeds")
                                     : null;

                Scene.Box          = C.BoxEsp;
                Scene.Name         = C.NameEsp;
                Scene.HealthBar    = C.HealthBar;
                Scene.Distance     = C.DistanceEsp;
                Scene.Snapline     = C.Snaplines;
                Scene.BoxStyle     = C.BoxStyle;
                Scene.BoxThickness = C.BoxThickness;
                Scene.ColorVisible = C.BoxVisible;
                Scene.ColorHidden  = C.BoxHidden;

                Scene.Crosshair      = C.Crosshair;
                Scene.CrosshairColor = C.CrosshairCol;
                Scene.Fov            = C.AimFov;
                Scene.FovRadius      = C.AimFovSize * 0.5f;
                Scene.FovColor       = C.FovCol;

                Scene.Radar       = C.Radar;
                Scene.RadarSize   = C.RadarSize * 1.9f;
                Scene.RadarSquare = C.RadarSquare;
                Scene.RadarBg     = C.RadarBg;
                Scene.RadarFrame  = C.RadarFrame;

                DemoScene.Update(Scene);
            };

            var app = new OverlayApp(m, Scene);

            // "hidden" als Argument: startet mit geschlossenem Menue, dann sieht
            // man nur ESP, Radar, FOV und Watermark ueber dem Spiel
            if (args != null && Array.IndexOf(args, "hidden") >= 0)
                app.MenuVisible = false;

            app.Start().Wait();
        }
    }
}
