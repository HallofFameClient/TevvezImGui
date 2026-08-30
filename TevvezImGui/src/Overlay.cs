using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace TevvezImGui
{
    /// <summary>Ein Gegner, so wie ihn das Overlay zeichnen soll.</summary>
    public struct EspEntry
    {
        public Vector2 BoxMin, BoxMax;   // Bildschirmkoordinaten
        public string  Name;
        public float   Health, HealthMax;
        public float   Distance;
        public bool    Visible;          // sichtbar oder hinter einer Wand
    }

    /// <summary>Ein Punkt auf dem Radar, relativ zur Mitte in Pixeln.</summary>
    public struct RadarBlip
    {
        public Vector2 Offset;
        public int     Kind;             // 0 normal, 1 stark, 2 Boss
    }

    /// <summary>
    /// Alles, was das Overlay ueber das Spielbild zeichnet. Du fuellst die
    /// Felder mit deinen Daten, das Overlay malt sie.
    ///
    /// Bewusst getrennt vom Menue: das Menue stellt ein, die Szene zeigt an.
    /// </summary>
    public sealed class OverlayScene
    {
        // ---- ESP ---------------------------------------------------------
        public readonly List<EspEntry> Entities = new List<EspEntry>();

        public bool    Box        = true;
        public bool    Name       = true;
        public bool    HealthBar  = true;
        public bool    Distance   = true;
        public int     Snapline;                 // 0 aus, 1 unten, 2 mitte, 3 oben
        public float   BoxThickness = 2f;
        public int     BoxStyle;                 // 0 Ecken, 1 Rechteck
        public Vector4 ColorVisible = Theme.Rgb(0xA8, 0x55, 0xF7);
        public Vector4 ColorHidden  = Theme.Rgb(0x4A, 0x2E, 0x8F);

        // ---- Fadenkreuz und FOV -----------------------------------------
        public bool    Crosshair     = true;
        public Vector4 CrosshairColor = Theme.Rgb(0xE0, 0xAA, 0xFF);
        public bool    Fov           = true;
        public float   FovRadius     = 220f;
        public bool    FovSimple;
        public Vector4 FovColor      = Theme.Rgb(0xC7, 0x7D, 0xFF, 0.70f);

        // ---- Radar -------------------------------------------------------
        public readonly List<RadarBlip> Blips = new List<RadarBlip>();
        public bool    Radar       = true;
        public Vector2 RadarPos    = new Vector2(24, 90);
        public float   RadarSize   = 190f;
        public bool    RadarSquare;
        public float   RadarBg     = 0.55f;
        public Vector4 RadarFrame  = Theme.Rgb(0xA8, 0x76, 0xF7, 0.55f);

        // =================================================================
        public void Draw(ImDrawListPtr dl, Vector2 origin, Vector2 size)
        {
            Vector2 center = origin + size * 0.5f;

            DrawFov(dl, center);
            DrawEsp(dl);
            DrawRadar(dl, origin);
            DrawCrosshair(dl, center);
        }

        private void DrawFov(ImDrawListPtr dl, Vector2 center)
        {
            if (!Fov || FovRadius < 4f) return;

            if (FovSimple)
            {
                dl.AddCircle(center, FovRadius, Theme.U32(FovColor), 96, 1.4f);
                return;
            }

            dl.AddCircleFilled(center, FovRadius, Theme.U32(Theme.A(FovColor, FovColor.W * 0.06f)), 96);
            dl.AddCircle(center, FovRadius, Theme.U32(FovColor), 96, 1.6f);

            // vier kleine Marken auf den Achsen
            for (int i = 0; i < 4; i++)
            {
                double a = i * Math.PI * 0.5;
                var d = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
                dl.AddLine(center + d * (FovRadius - 8f), center + d * (FovRadius + 8f),
                           Theme.U32(FovColor), 1.6f);
            }
        }

        private void DrawCrosshair(ImDrawListPtr dl, Vector2 center)
        {
            if (!Crosshair) return;
            uint col = Theme.U32(CrosshairColor);
            uint sh  = ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.62f));
            const float s = 10f;

            dl.AddLine(new Vector2(center.X - s, center.Y + 1), new Vector2(center.X - 2, center.Y + 1), sh, 3f);
            dl.AddLine(new Vector2(center.X + 2, center.Y + 1), new Vector2(center.X + s, center.Y + 1), sh, 3f);
            dl.AddLine(new Vector2(center.X + 1, center.Y - s), new Vector2(center.X + 1, center.Y - 2), sh, 3f);
            dl.AddLine(new Vector2(center.X + 1, center.Y + 2), new Vector2(center.X + 1, center.Y + s), sh, 3f);

            dl.AddLine(new Vector2(center.X - s, center.Y), new Vector2(center.X - 2, center.Y), col, 1.6f);
            dl.AddLine(new Vector2(center.X + 2, center.Y), new Vector2(center.X + s, center.Y), col, 1.6f);
            dl.AddLine(new Vector2(center.X, center.Y - s), new Vector2(center.X, center.Y - 2), col, 1.6f);
            dl.AddLine(new Vector2(center.X, center.Y + 2), new Vector2(center.X, center.Y + s), col, 1.6f);
        }

        private void DrawEsp(ImDrawListPtr dl)
        {
            var font = ImGui.GetFont();
            float fs = ImGui.GetFontSize();

            foreach (var e in Entities)
            {
                Vector4 col = e.Visible ? ColorVisible : ColorHidden;
                uint c = Theme.U32(col);
                uint shadow = ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.55f));

                if (Box)
                {
                    if (BoxStyle == 1)
                    {
                        dl.AddRect(e.BoxMin + new Vector2(1, 1), e.BoxMax + new Vector2(1, 1), shadow, 0, 0, BoxThickness);
                        dl.AddRect(e.BoxMin, e.BoxMax, c, 0, 0, BoxThickness);
                    }
                    else
                    {
                        DrawCorners(dl, e.BoxMin, e.BoxMax, c, BoxThickness);
                    }
                }

                if (HealthBar && e.HealthMax > 0f)
                {
                    float pct = e.Health / e.HealthMax;
                    if (pct < 0f) pct = 0f; if (pct > 1f) pct = 1f;

                    var bm = new Vector2(e.BoxMin.X - 7f, e.BoxMin.Y);
                    var bx = new Vector2(e.BoxMin.X - 3f, e.BoxMax.Y);
                    dl.AddRectFilled(bm, bx, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.65f)), 2f);

                    Vector4 hc = pct > 0.6f ? Theme.Rgb(0x3D, 0xCC, 0x59)
                               : pct > 0.3f ? Theme.Rgb(0xFF, 0xC2, 0x29)
                                            : Theme.Rgb(0xF0, 0x3D, 0x33);
                    float h = (bx.Y - bm.Y) * pct;
                    dl.AddRectFilled(new Vector2(bm.X, bx.Y - h), bx, Theme.U32(hc), 2f);
                }

                if (Snapline != 0)
                {
                    var io = ImGui.GetIO();
                    Vector2 from;
                    if (Snapline == 1)      from = new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y);
                    else if (Snapline == 2) from = io.DisplaySize * 0.5f;
                    else                    from = new Vector2(io.DisplaySize.X * 0.5f, 0f);
                    var to = new Vector2((e.BoxMin.X + e.BoxMax.X) * 0.5f, e.BoxMax.Y);
                    dl.AddLine(from, to, Theme.U32(Theme.A(col, 0.35f)), 1.2f);
                }

                if (Name && !string.IsNullOrEmpty(e.Name))
                {
                    Vector2 ts = font.CalcTextSizeA(fs, float.MaxValue, 0f, e.Name);
                    var p = new Vector2((e.BoxMin.X + e.BoxMax.X) * 0.5f - ts.X * 0.5f, e.BoxMin.Y - ts.Y - 4f);
                    dl.AddText(font, fs, p + new Vector2(1, 1), shadow, e.Name);
                    dl.AddText(font, fs, p, Theme.U32(Theme.Text), e.Name);
                }

                if (Distance)
                {
                    string s = ((int)e.Distance) + " m";
                    if (HealthBar && e.HealthMax > 0f) s = ((int)e.Health) + " HP  -  " + s;
                    Vector2 ts = font.CalcTextSizeA(fs * 0.9f, float.MaxValue, 0f, s);
                    var p = new Vector2((e.BoxMin.X + e.BoxMax.X) * 0.5f - ts.X * 0.5f, e.BoxMax.Y + 3f);
                    dl.AddText(font, fs * 0.9f, p + new Vector2(1, 1), shadow, s);
                    dl.AddText(font, fs * 0.9f, p, Theme.U32(Theme.TextDim), s);
                }
            }
        }

        private static void DrawCorners(ImDrawListPtr dl, Vector2 mn, Vector2 mx, uint c, float th)
        {
            float w = (mx.X - mn.X) * 0.28f;
            float h = (mx.Y - mn.Y) * 0.22f;

            dl.AddLine(mn, new Vector2(mn.X + w, mn.Y), c, th);
            dl.AddLine(mn, new Vector2(mn.X, mn.Y + h), c, th);

            dl.AddLine(new Vector2(mx.X, mn.Y), new Vector2(mx.X - w, mn.Y), c, th);
            dl.AddLine(new Vector2(mx.X, mn.Y), new Vector2(mx.X, mn.Y + h), c, th);

            dl.AddLine(new Vector2(mn.X, mx.Y), new Vector2(mn.X + w, mx.Y), c, th);
            dl.AddLine(new Vector2(mn.X, mx.Y), new Vector2(mn.X, mx.Y - h), c, th);

            dl.AddLine(mx, new Vector2(mx.X - w, mx.Y), c, th);
            dl.AddLine(mx, new Vector2(mx.X, mx.Y - h), c, th);
        }

        private void DrawRadar(ImDrawListPtr dl, Vector2 origin)
        {
            if (!Radar || RadarSize < 40f) return;

            Vector2 mn = origin + RadarPos;
            Vector2 mx = mn + new Vector2(RadarSize, RadarSize);
            Vector2 c  = (mn + mx) * 0.5f;
            float r = RadarSize * 0.5f;

            if (RadarBg > 0.02f)
            {
                uint bg = Theme.U32(Theme.A(Theme.Bg, RadarBg));
                if (RadarSquare) dl.AddRectFilled(mn, mx, bg, 6f);
                else             dl.AddCircleFilled(c, r, bg, 64);
            }

            uint fr = Theme.U32(RadarFrame);
            if (RadarSquare) dl.AddRect(mn, mx, fr, 6f, 0, 1.4f);
            else             dl.AddCircle(c, r, fr, 64, 1.4f);

            uint cross = Theme.U32(Theme.A(RadarFrame, RadarFrame.W * 0.55f));
            dl.AddLine(new Vector2(c.X, mn.Y + 4f), new Vector2(c.X, mx.Y - 4f), cross, 1f);
            dl.AddLine(new Vector2(mn.X + 4f, c.Y), new Vector2(mx.X - 4f, c.Y), cross, 1f);

            foreach (var b in Blips)
            {
                Vector2 p = c + b.Offset;
                if (RadarSquare)
                {
                    if (p.X < mn.X + 3 || p.X > mx.X - 3 || p.Y < mn.Y + 3 || p.Y > mx.Y - 3) continue;
                }
                else if (Vector2.Distance(p, c) > r - 3f) continue;

                Vector4 bc = b.Kind == 2 ? Theme.Rgb(0xFF, 0x45, 0x6D)
                           : b.Kind == 1 ? Theme.Rgb(0xE0, 0x9D, 0xFF)
                                         : Theme.Rgb(0xA8, 0x55, 0xF7);
                dl.AddCircleFilled(p, 3.2f, Theme.U32(bc), 10);
            }

            // eigene Position
            dl.AddTriangleFilled(new Vector2(c.X, c.Y - 6f), new Vector2(c.X - 4.5f, c.Y + 4f),
                                 new Vector2(c.X + 4.5f, c.Y + 4f), Theme.U32(Theme.Text, 0.95f));
        }
    }
}
