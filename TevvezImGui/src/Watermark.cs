using System;
using System.Numerics;
using ImGuiNET;

namespace TevvezImGui
{
    /// <summary>
    /// Der Balken oben am Bildschirmrand - dasselbe Aussehen wie in der DLL.
    /// Gezeichnet wird auf die Vordergrundliste, damit er ueber dem Menue liegt.
    ///
    ///     m.Watermark.Show = true;
    ///     m.Watermark.Extra = () =&gt; zeds + " zeds";
    /// </summary>
    public sealed class Watermark
    {
        public bool   Show;
        public string Label    = "Tevvez Cheats";
        public int    Position;            // 0 = links, 1 = mitte, 2 = rechts
        public bool   ShowLogo = true;
        public bool   ShowFps  = true;
        public bool   ShowTime = true;

        /// <summary>Beliebiger Zusatz, z.B. die Zahl der Gegner. null = weglassen.</summary>
        public Func<string> Extra;

        public void Draw(ImDrawListPtr dl, Vector2 origin, float screenWidth)
        {
            if (!Show) return;

            var font = ImGui.GetFont();
            float fs = ImGui.GetFontSize();

            // rechter Teil: FPS, Zusatz, Uhrzeit
            string right = "";
            if (ShowFps)
                right = ((int)Math.Round(ImGui.GetIO().Framerate)) + " fps";
            if (Extra != null)
            {
                string ex = null;
                try { ex = Extra(); } catch { }
                if (!string.IsNullOrEmpty(ex))
                    right += (right.Length > 0 ? "  |  " : "") + ex;
            }
            if (ShowTime)
                right += (right.Length > 0 ? "  |  " : "") + DateTime.Now.ToString("HH:mm");

            Vector2 ls = font.CalcTextSizeA(fs, float.MaxValue, 0f, Label);
            Vector2 rs = right.Length > 0
                       ? font.CalcTextSizeA(fs * 0.92f, float.MaxValue, 0f, right)
                       : Vector2.Zero;

            float padX = 12f, padY = 6f;
            float gap  = right.Length > 0 ? 12f : 0f;
            float markW = ShowLogo ? 30f : 0f;
            float w = padX * 2f + markW + ls.X + gap + rs.X + 4f;
            float h = fs + padY * 2f;

            float x = 14f;
            if      (Position == 1) x = (screenWidth - w) * 0.5f;
            else if (Position == 2) x = screenWidth - w - 14f;
            float y = 12f;

            Vector2 a = new Vector2(origin.X + x, origin.Y + y);
            Vector2 b = new Vector2(a.X + w, a.Y + h);

            dl.AddRectFilled(a, b, Theme.U32(Theme.Bg, 0.82f), 7f);
            dl.AddRect(a, b, Theme.U32(Theme.Accent, 0.35f), 7f, 0, 1f);
            dl.AddRectFilled(new Vector2(a.X, a.Y + 5f), new Vector2(a.X + 3f, b.Y - 5f),
                             Theme.U32(Theme.Accent, 0.95f), 2f);

            if (ShowLogo)
                Logo.DrawHead(dl, new Vector2(a.X + padX - 3f, a.Y + 3f),
                                  new Vector2(a.X + padX + markW - 9f, b.Y - 3f));

            dl.AddText(font, fs, new Vector2(a.X + padX + markW, a.Y + padY),
                       Theme.U32(Theme.Text, 1f), Label);

            if (right.Length > 0)
                dl.AddText(font, fs * 0.92f,
                           new Vector2(a.X + padX + markW + ls.X + gap, a.Y + padY + 1f),
                           Theme.U32(Theme.Accent, 0.95f), right);
        }
    }
}
