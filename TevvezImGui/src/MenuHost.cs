using System;
using System.IO;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace TevvezImGui
{
    /// <summary>
    /// Fenster und Renderschleife. Oeffnet ein rahmenloses Fenster, richtet
    /// ImGui ein und laesst das Menue jeden Frame zeichnen.
    ///
    /// Rahmenlos deshalb, weil das Menue seine eigene Kopfzeile mitbringt -
    /// eine Windows-Titelleiste darueber saehe fremd aus.
    /// </summary>
    public sealed class MenuHost : IDisposable
    {
        private readonly Menu _menu;
        private IWindow _window;
        private GL _gl;
        private IInputContext _input;
        private ImGuiController _controller;
        private uint _logoTex;

        public MenuHost(Menu menu, int width = 950, int height = 660)
        {
            _menu = menu;

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(width, height);
            options.Title = menu.Title;
            options.WindowBorder = WindowBorder.Hidden;
            options.VSync = true;
            options.PreferredDepthBufferBits = 0;

            _window = Window.Create(options);
            _window.Load              += OnLoad;
            _window.Render            += OnRender;
            _window.FramebufferResize += s => { if (_gl != null) _gl.Viewport(s); };
            _window.Closing           += OnClosing;

            _menu.OnClose      = () => _window.Close();
            _menu.OnDragWindow = d =>
            {
                _window.Position = new Vector2D<int>(
                    _window.Position.X + (int)Math.Round(d.X),
                    _window.Position.Y + (int)Math.Round(d.Y));
            };
            _menu.OnResizeWindow = d =>
            {
                int w = Math.Max(660, _window.Size.X + (int)Math.Round(d.X));
                int h = Math.Max(470, _window.Size.Y + (int)Math.Round(d.Y));
                _window.Size = new Vector2D<int>(w, h);
            };
        }

        public void Run() { _window.Run(); }

        private void OnLoad()
        {
            _gl    = _window.CreateOpenGL();
            _input = _window.CreateInput();

            // Segoe UI, falls vorhanden - sonst die eingebaute ImGui-Schrift
            string font = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf");
            if (File.Exists(font))
                _controller = new ImGuiController(_gl, _window, _input, new ImGuiFontConfig(font, 17));
            else
                _controller = new ImGuiController(_gl, _window, _input);

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            // keine imgui.ini im Programmordner anlegen
            unsafe { io.NativePtr->IniFilename = null; }

            Theme.Apply();
            TryLoadLogo();

            // ESC schliesst
            foreach (var kb in _input.Keyboards)
                kb.KeyDown += (k, key, code) => { if (key == Key.Escape) _window.Close(); };
        }

        private void OnRender(double dt)
        {
            _controller.Update((float)dt);

            _gl.ClearColor(Theme.Bg.X, Theme.Bg.Y, Theme.Bg.Z, 1f);
            _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            _menu.Update();

            // Ist das Watermark an, ruecken wir das Menue darunter - im Spiel liegt
            // es ueber dem Bild, hier ueber dem eigenen Fenster.
            float top = _menu.Watermark.Show ? 46f : 0f;
            var screen = new Vector2(_window.Size.X, _window.Size.Y);
            _menu.Draw(new Vector2(0, top), new Vector2(screen.X, screen.Y - top));
            _menu.DrawWatermark(Vector2.Zero, screen.X);

            _controller.Render();
        }

        private void OnClosing()
        {
            if (_logoTex != 0) { _gl.DeleteTexture(_logoTex); _logoTex = 0; }
            if (_controller != null) _controller.Dispose();
            if (_input != null) _input.Dispose();
            if (_gl != null) _gl.Dispose();
        }

        /// <summary>
        /// Laedt tevvez_logo.png (oder logo.png) neben der Anwendung und legt es
        /// als Textur ab. Fehlt die Datei, zeichnet Logo die Eule selbst.
        /// </summary>
        private void TryLoadLogo()
        {
            string[] names = { "tevvez_logo.png", "logo.png" };
            foreach (var n in names)
            {
                string path = Path.Combine(AppContext.BaseDirectory, n);
                if (!File.Exists(path)) continue;
                try
                {
                    using (var stream = File.OpenRead(path))
                    {
                        var img = StbImageSharp.ImageResult.FromStream(
                            stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

                        _logoTex = _gl.GenTexture();
                        _gl.BindTexture(TextureTarget.Texture2D, _logoTex);
                        unsafe
                        {
                            fixed (byte* p = img.Data)
                            {
                                _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba8,
                                    (uint)img.Width, (uint)img.Height, 0,
                                    PixelFormat.Rgba, PixelType.UnsignedByte, p);
                            }
                        }
                        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
                        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
                        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
                        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
                        _gl.BindTexture(TextureTarget.Texture2D, 0);

                        Logo.Texture = (nint)_logoTex;
                        Logo.TextureSize = new Vector2(img.Width, img.Height);
                        return;
                    }
                }
                catch { /* egal - dann eben die gezeichnete Eule */ }
            }
        }

        public void Dispose()
        {
            if (_window != null) { _window.Dispose(); _window = null; }
        }
    }

    /// <summary>
    /// Startet das Menue in einem eigenen Fenster. Liegt hier und nicht in Menu,
    /// weil nur diese Datei Silk.NET braucht - das Overlay-Projekt kommt ohne aus.
    /// </summary>
    public static class MenuWindow
    {
        public static void Run(this Menu menu, int width = 950, int height = 660)
        {
            using (var host = new MenuHost(menu, width, height))
                host.Run();
        }
    }
}
