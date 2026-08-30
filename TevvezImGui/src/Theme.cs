using System.Numerics;
using ImGuiNET;

namespace TevvezImGui
{
    /// <summary>
    /// Farben, Masse und kleine Helfer. Alles an einer Stelle - wer ein anderes
    /// Aussehen will, aendert nur diese Datei.
    /// </summary>
    public static class Theme
    {
        // ---- Farben (0..1 wie in ImGui ueblich) --------------------------
        public static Vector4 Accent  = Rgb(0x8B, 0x44, 0xC8);   // Logo-Violett
        public static Vector4 Bg      = Rgb(0x0A, 0x08, 0x13);
        public static Vector4 Panel   = Rgb(0x10, 0x0D, 0x1E);
        public static Vector4 Card    = Rgb(0x16, 0x12, 0x2B);
        public static Vector4 Frame   = Rgb(0x23, 0x1C, 0x40);
        public static Vector4 Text    = Rgb(0xED, 0xE7, 0xF7);
        public static Vector4 TextDim = Rgb(0x8F, 0x84, 0xB5);
        public static Vector4 Line    = Rgb(0xC7, 0xB3, 0xFF);

        public static float MenuAlpha = 0.98f;

        public static readonly Vector4[] AccentPresets =
        {
            Rgb(0x8B, 0x44, 0xC8),   // Logo-Violett
            Rgb(0xE0, 0x9D, 0xFF),   // Augen-Violett
            Rgb(0x67, 0x43, 0xC7),   // Indigo
            Rgb(0xFA, 0x6B, 0xB8),
            Rgb(0x3D, 0xB8, 0xFA),
            Rgb(0x5C, 0xD1, 0x70),
            Rgb(0xFF, 0x9E, 0x1A),
        };

        // ---- Masse -------------------------------------------------------
        public const float HeaderHeight = 58f;
        public const float FooterHeight = 28f;
        public const float SidebarWidth = 186f;

        /// <summary>Ab dieser Inhaltsbreite wird zwei- bzw. dreispaltig gelegt.</summary>
        public const float TwoColumnWidth   = 600f;
        public const float ThreeColumnWidth = 1080f;

        // ---- Helfer ------------------------------------------------------
        public static Vector4 Rgb(int r, int g, int b, float a = 1f)
        {
            return new Vector4(r / 255f, g / 255f, b / 255f, a);
        }

        public static Vector4 A(Vector4 c, float alpha)
        {
            return new Vector4(c.X, c.Y, c.Z, alpha);
        }

        public static Vector4 Mix(Vector4 a, Vector4 b, float t)
        {
            if (t < 0f) t = 0f; if (t > 1f) t = 1f;
            return a + (b - a) * t;
        }

        public static Vector4 Shade(Vector4 c, float f)
        {
            return new Vector4(Sat(c.X * f), Sat(c.Y * f), Sat(c.Z * f), c.W);
        }

        private static float Sat(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }

        /// <summary>Farbe zu ImGui-Farbwert. alpha wird zusaetzlich multipliziert.</summary>
        public static uint U32(Vector4 c, float alpha = -1f)
        {
            float a = (alpha < 0f) ? c.W : alpha;
            return ImGui.ColorConvertFloat4ToU32(new Vector4(c.X, c.Y, c.Z, a * ImGui.GetStyle().Alpha));
        }

        /// <summary>Setzt den kompletten ImGui-Stil. Muss nach jedem Farbwechsel laufen.</summary>
        public static void Apply()
        {
            var s = ImGui.GetStyle();

            s.WindowPadding     = new Vector2(0, 0);
            s.FramePadding      = new Vector2(10, 6);
            s.ItemSpacing       = new Vector2(8, 7);
            s.ItemInnerSpacing  = new Vector2(7, 5);
            s.CellPadding       = new Vector2(10, 4);
            s.ScrollbarSize     = 11f;
            s.GrabMinSize       = 11f;

            s.WindowRounding    = 12f;
            s.ChildRounding     = 10f;
            s.FrameRounding     = 6f;
            s.PopupRounding     = 8f;
            s.ScrollbarRounding = 6f;
            s.GrabRounding      = 5f;
            s.TabRounding       = 7f;

            s.WindowBorderSize  = 1f;
            s.ChildBorderSize   = 1f;
            s.FrameBorderSize   = 0f;
            s.PopupBorderSize   = 1f;

            s.WindowTitleAlign  = new Vector2(0.5f, 0.5f);

            var c = s.Colors;
            c[(int)ImGuiCol.Text]                  = Text;
            c[(int)ImGuiCol.TextDisabled]          = TextDim;
            c[(int)ImGuiCol.WindowBg]              = A(Bg, MenuAlpha);
            c[(int)ImGuiCol.ChildBg]               = new Vector4(0, 0, 0, 0);
            c[(int)ImGuiCol.PopupBg]               = A(Panel, 0.98f);
            c[(int)ImGuiCol.Border]                = A(Line, 0.09f);
            c[(int)ImGuiCol.BorderShadow]          = new Vector4(0, 0, 0, 0);

            c[(int)ImGuiCol.FrameBg]               = Frame;
            c[(int)ImGuiCol.FrameBgHovered]        = Mix(Frame, Accent, 0.18f);
            c[(int)ImGuiCol.FrameBgActive]         = Mix(Frame, Accent, 0.28f);

            c[(int)ImGuiCol.TitleBg]               = Panel;
            c[(int)ImGuiCol.TitleBgActive]         = Panel;
            c[(int)ImGuiCol.TitleBgCollapsed]      = Panel;
            c[(int)ImGuiCol.MenuBarBg]             = Panel;

            c[(int)ImGuiCol.ScrollbarBg]           = new Vector4(0, 0, 0, 0);
            c[(int)ImGuiCol.ScrollbarGrab]         = A(Line, 0.14f);
            c[(int)ImGuiCol.ScrollbarGrabHovered]  = A(Line, 0.24f);
            c[(int)ImGuiCol.ScrollbarGrabActive]   = A(Line, 0.34f);

            c[(int)ImGuiCol.CheckMark]             = Accent;
            c[(int)ImGuiCol.SliderGrab]            = Accent;
            c[(int)ImGuiCol.SliderGrabActive]      = Shade(Accent, 1.15f);

            c[(int)ImGuiCol.Button]                = Frame;
            c[(int)ImGuiCol.ButtonHovered]         = Mix(Frame, Accent, 0.30f);
            c[(int)ImGuiCol.ButtonActive]          = Mix(Frame, Accent, 0.45f);

            c[(int)ImGuiCol.Header]                = Mix(Frame, Accent, 0.22f);
            c[(int)ImGuiCol.HeaderHovered]         = Mix(Frame, Accent, 0.34f);
            c[(int)ImGuiCol.HeaderActive]          = Mix(Frame, Accent, 0.46f);

            c[(int)ImGuiCol.Separator]             = A(Line, 0.08f);
            c[(int)ImGuiCol.SeparatorHovered]      = A(Accent, 0.5f);
            c[(int)ImGuiCol.SeparatorActive]       = A(Accent, 0.8f);

            c[(int)ImGuiCol.ResizeGrip]            = A(Line, 0.10f);
            c[(int)ImGuiCol.ResizeGripHovered]     = A(Accent, 0.55f);
            c[(int)ImGuiCol.ResizeGripActive]      = A(Accent, 0.85f);

            c[(int)ImGuiCol.Tab]                   = Panel;
            c[(int)ImGuiCol.TabHovered]            = Mix(Panel, Accent, 0.35f);
            c[(int)ImGuiCol.TabSelected]           = Mix(Panel, Accent, 0.22f);

            c[(int)ImGuiCol.TableHeaderBg]         = Panel;
            c[(int)ImGuiCol.TableBorderStrong]     = A(Line, 0.12f);
            c[(int)ImGuiCol.TableBorderLight]      = A(Line, 0.06f);
            c[(int)ImGuiCol.TableRowBg]            = new Vector4(0, 0, 0, 0);
            c[(int)ImGuiCol.TableRowBgAlt]         = A(Line, 0.02f);

            c[(int)ImGuiCol.TextSelectedBg]        = A(Accent, 0.35f);
            c[(int)ImGuiCol.NavCursor]             = A(Accent, 0.85f);
        }
    }
}
