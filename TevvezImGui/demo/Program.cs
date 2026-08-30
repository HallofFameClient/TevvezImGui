using System;

namespace TevvezImGui.Demo
{
    /// <summary>
    /// Startet das Menue in einem eigenen Fenster.
    /// Der Aufbau selbst steht in MenuBuilder - denselben benutzt auch das
    /// Overlay-Projekt, damit beide genau dasselbe Menue zeigen.
    /// </summary>
    internal static class Program
    {
        private static readonly Settings C = new Settings();
        private const string CfgPath = "tevvez.cfg";

        [STAThread]
        private static void Main(string[] args)
        {
            ConfigStore.Load(C, CfgPath);

            var m = MenuBuilder.Build(C, CfgPath);

            // Nach jeder Aenderung speichern
            m.Changed = () => ConfigStore.Save(C, CfgPath);

            // Watermark mit den Einstellungen verbinden
            m.BeforeDraw = () =>
            {
                m.Watermark.Show     = C.Watermark;
                m.Watermark.Position = C.WatermarkPos;
                m.Watermark.ShowFps  = C.WatermarkFps;
                m.Watermark.ShowTime = C.WatermarkTime;
                m.Watermark.Extra    = C.WatermarkZeds ? (Func<string>)(() => "12 zeds") : null;
            };

            // Startseite ueber die Befehlszeile waehlen: TevvezImGui.Demo.exe 2
            int startTab;
            if (args != null && args.Length > 0 && int.TryParse(args[0], out startTab))
                m.StartTab(startTab);

            m.Run(950, 660);
        }
    }
}
