using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace TevvezImGui.OverlayDemo
{
    /// <summary>
    /// Durchsichtiges Overlay ueber dem ganzen Bildschirm.
    ///
    /// Die schwierigen Teile - transparentes Fenster, immer im Vordergrund,
    /// Klicks durchlassen wenn nichts angezeigt wird - kommen von
    /// ClickableTransparentOverlay. Solange kein ImGui-Fenster offen ist, gehen
    /// alle Klicks an das Spiel darunter. Sobald das Menue sichtbar ist, faengt
    /// das Overlay die Maus ab.
    ///
    /// INSERT blendet das Menue ein und aus, END beendet das Overlay.
    /// Beide werden ueber GetAsyncKeyState abgefragt, damit sie auch dann
    /// ankommen, wenn das Overlay keinen Tastaturfokus hat.
    /// </summary>
    public sealed class OverlayApp : Overlay
    {
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")] private static extern int GetSystemMetrics(int index);
        private const int VK_INSERT = 0x2D, VK_END = 0x23;
        private const int SM_CXSCREEN = 0, SM_CYSCREEN = 1;

        private static int ScreenW { get { int v = GetSystemMetrics(SM_CXSCREEN); return v > 0 ? v : 1920; } }
        private static int ScreenH { get { int v = GetSystemMetrics(SM_CYSCREEN); return v > 0 ? v : 1080; } }

        private readonly Menu         _menu;
        private readonly OverlayScene _scene;

        /// <summary>Ist das Menue beim Start sichtbar?</summary>
        public bool MenuVisible { get { return _menuVisible; } set { _menuVisible = value; } }

        private bool _menuVisible = true;
        private bool _insertWasDown, _endWasDown;

        public OverlayApp(Menu menu, OverlayScene scene)
            : base("Tevvez Overlay", true, ScreenW, ScreenH)
        {
            _menu  = menu;
            _scene = scene;

            // Im Overlay ist das Menue ein bewegliches Fenster, kein Vollbild
            _menu.Floating = true;
            _menu.OnClose  = () => _menuVisible = false;
            _menu.Footer("INSERT  Menue    END  beenden");
        }

        protected override Task PostInitialized()
        {
            // Erst hier - vorher gibt es das Fenster noch nicht
            Position = new System.Drawing.Point(0, 0);
            Size     = new System.Drawing.Size(ScreenW, ScreenH);
            FPSLimit = 60;          // mehr braucht ein Overlay nicht

            // Schrift: Segoe UI, falls vorhanden
            string font = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf");
            if (File.Exists(font))
                ReplaceFont(font, 17, FontGlyphRangeType.English);

            Theme.Apply();

            // keine imgui.ini im Arbeitsverzeichnis anlegen - im injizierten Fall
            // waere das der Ordner des fremden Prozesses
            unsafe { ImGui.GetIO().NativePtr->IniFilename = null; }

            // Logo als Textur - das Overlay bringt dafuer schon alles mit
            string logo = Path.Combine(AppContext.BaseDirectory, "tevvez_logo.png");
            if (File.Exists(logo))
            {
                nint handle; uint w, h;
                AddOrGetImagePointer(logo, true, out handle, out w, out h);
                Logo.Texture     = handle;
                Logo.TextureSize = new Vector2(w, h);
            }

            return Task.CompletedTask;
        }

        private bool Pressed(int vk, ref bool wasDown)
        {
            bool down = (GetAsyncKeyState(vk) & 0x8000) != 0;
            bool hit  = down && !wasDown;
            wasDown = down;
            return hit;
        }

        protected override void Render()
        {
            if (Pressed(VK_END, ref _endWasDown)) { Close(); return; }
            if (Pressed(VK_INSERT, ref _insertWasDown)) _menuVisible = !_menuVisible;

            Vector2 screen = ImGui.GetIO().DisplaySize;

            _menu.Update();

            // Spielelemente ganz nach hinten, damit das Menue darueber liegt
            _scene.Draw(ImGui.GetBackgroundDrawList(), Vector2.Zero, screen);

            // Watermark nach vorn - im Spiel liegt es ueber allem
            _menu.DrawWatermark(Vector2.Zero, screen.X);

            // Nur wenn das Menue offen ist, entsteht ein ImGui-Fenster - und nur
            // dann faengt das Overlay ueberhaupt Mausklicks ab.
            if (_menuVisible)
            {
                var size = new Vector2(950, 660);
                var pos  = new Vector2((screen.X - size.X) * 0.5f, (screen.Y - size.Y) * 0.5f);
                _menu.Draw(pos, size);
            }
        }
    }
}
