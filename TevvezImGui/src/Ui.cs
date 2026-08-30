using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using ImGuiNET;

namespace TevvezImGui
{
    /// <summary>
    /// Die Bedienelemente, direkt mit ImGui gezeichnet.
    /// Immediate Mode: jede Methode zeichnet und wertet in einem Aufruf aus und
    /// meldet zurueck, ob sich etwas geaendert hat.
    /// </summary>
    public static class Ui
    {
        // Weiche Uebergaenge je Element. ImGui hat keinen Zustand dafuer,
        // deshalb eine kleine Tabelle ueber die Element-ID.
        private static readonly Dictionary<uint, float> Anims = new Dictionary<uint, float>();

        private static float Anim(uint id, bool on, float speed = 14f)
        {
            float target = on ? 1f : 0f;
            float v;
            if (!Anims.TryGetValue(id, out v)) v = target;
            float dt = ImGui.GetIO().DeltaTime;
            if (dt > 0.1f) dt = 0.1f;
            v += (target - v) * Math.Min(1f, dt * speed);
            if (Math.Abs(target - v) < 0.002f) v = target;
            Anims[id] = v;
            return v;
        }

        private static void Tip(string tooltip)
        {
            if (!string.IsNullOrEmpty(tooltip) && ImGui.IsItemHovered())
                ImGui.SetTooltip(tooltip);
        }

        // =================================================================
        //  Schalter
        // =================================================================
        public static bool Toggle(string label, ref bool value, string tooltip = null)
        {
            ImGui.PushID(label);
            var dl    = ImGui.GetWindowDrawList();
            float avail = ImGui.GetContentRegionAvail().X;
            float rowH  = ImGui.GetFrameHeight() + 4f;
            float sh    = ImGui.GetFrameHeight() * 0.74f;
            float sw    = sh * 1.85f;

            Vector2 p = ImGui.GetCursorScreenPos();
            bool pressed = ImGui.InvisibleButton("##hit", new Vector2(avail, rowH));
            bool hovered = ImGui.IsItemHovered();
            if (pressed) value = !value;

            uint id = ImGui.GetID("##hit");
            float t = Anim(id, value);

            if (hovered)
                dl.AddRectFilled(new Vector2(p.X - 8f, p.Y), new Vector2(p.X + avail + 8f, p.Y + rowH),
                                 Theme.U32(Theme.Line, 0.045f), 6f);

            Vector2 mn = new Vector2(p.X + avail - sw, p.Y + (rowH - sh) * 0.5f);
            DrawSwitch(dl, mn, new Vector2(mn.X + sw, mn.Y + sh), t, hovered);

            float fs = ImGui.GetFontSize();
            dl.AddText(new Vector2(p.X, p.Y + (rowH - fs) * 0.5f),
                       Theme.U32(Theme.Mix(Theme.TextDim, Theme.Text, 0.30f + 0.70f * t)), label);

            Tip(tooltip);
            ImGui.PopID();
            return pressed;
        }

        /// <summary>Der Schalter selbst - Pille mit Knopf.</summary>
        public static void DrawSwitch(ImDrawListPtr dl, Vector2 mn, Vector2 mx, float t, bool hovered)
        {
            float h = mx.Y - mn.Y;
            float r = h * 0.5f;
            dl.AddRectFilled(mn, mx, Theme.U32(Theme.Mix(Theme.Frame, Theme.Accent, t)), r);
            dl.AddRect(mn, mx, Theme.U32(Theme.Line, hovered ? 0.30f : 0.12f), r, 0, 1f);

            float kr = r - 3f;
            float kx = mn.X + 3f + kr + (mx.X - mn.X - 6f - kr * 2f) * t;
            dl.AddCircleFilled(new Vector2(kx, mn.Y + r), kr, Theme.U32(new Vector4(1, 1, 1, 1), 0.95f), 18);
        }

        // =================================================================
        //  Regler
        // =================================================================
        public static bool Slider(string label, ref float value, float min, float max,
                                  string format = "0.##", string zeroText = null, string tooltip = null)
        {
            ImGui.PushID(label);
            float avail = ImGui.GetContentRegionAvail().X;

            string vt = (zeroText != null && Math.Abs(value) < 0.0001f)
                      ? zeroText
                      : value.ToString(format, CultureInfo.InvariantCulture);

            // Beschriftung und Zahlenwert brauchen Platz - der Rest gehoert dem Regler
            float labelW = ImGui.CalcTextSize(label).X;
            float valueW = ImGui.CalcTextSize(vt).X;
            float w = avail * 0.55f;
            if (w > 250f) w = 250f;
            float room = avail - labelW - valueW - 30f;
            if (w > room) w = room;
            if (w < 90f) w = 90f;

            Vector2 start = ImGui.GetCursorPos();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Theme.TextDim, label);

            ImGui.SetCursorPos(new Vector2(start.X + avail - w, start.Y));
            ImGui.SetNextItemWidth(w);
            ImGui.SliderFloat("##s", ref value, min, max, "", ImGuiSliderFlags.AlwaysClamp);
            bool changed = ImGui.IsItemDeactivatedAfterEdit();
            Tip(tooltip);

            // Zahlenwert links neben dem Regler - so verdeckt ihn der Griff nie
            Vector2 imn = ImGui.GetItemRectMin();
            Vector2 imx = ImGui.GetItemRectMax();
            Vector2 ts  = ImGui.CalcTextSize(vt);
            ImGui.GetWindowDrawList().AddText(
                new Vector2(imn.X - 12f - ts.X, imn.Y + (imx.Y - imn.Y - ts.Y) * 0.5f),
                Theme.U32(Theme.Mix(Theme.Text, Theme.Accent, 0.55f), 1f), vt);

            ImGui.PopID();
            return changed;
        }

        // =================================================================
        //  Auswahlfeld
        // =================================================================
        public static bool Combo(string label, ref int value, string[] items, string tooltip = null)
        {
            ImGui.PushID(label);
            float avail = ImGui.GetContentRegionAvail().X;
            float w = avail * 0.50f;
            if (w > 200f) w = 200f;
            if (w < 130f) w = 130f;

            Vector2 start = ImGui.GetCursorPos();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Theme.TextDim, label);

            ImGui.SetCursorPos(new Vector2(start.X + avail - w, start.Y));
            ImGui.SetNextItemWidth(w);

            int cur = value;
            if (items == null || items.Length == 0) { ImGui.PopID(); return false; }
            if (cur < 0 || cur >= items.Length) cur = 0;

            bool changed = false;
            if (ImGui.BeginCombo("##c", items[cur]))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    bool sel = (i == cur);
                    if (ImGui.Selectable(items[i], sel)) { value = i; changed = true; }
                    if (sel) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            Tip(tooltip);
            ImGui.PopID();
            return changed;
        }

        // =================================================================
        //  Farbfeld
        // =================================================================
        public static bool ColorField(string label, ref Vector4 value, string tooltip = null)
        {
            ImGui.PushID(label);
            float avail = ImGui.GetContentRegionAvail().X;
            Vector2 start = ImGui.GetCursorPos();

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Theme.TextDim, label);

            // Das Farbfeld ist quadratisch - rechtsbuendig, sonst laeuft es ins Label
            float sw = ImGui.GetFrameHeight();
            ImGui.SetCursorPos(new Vector2(start.X + avail - sw, start.Y));
            bool changed = ImGui.ColorEdit4("##c", ref value,
                ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.AlphaPreviewHalf |
                ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);
            Tip(tooltip);

            ImGui.PopID();
            return changed;
        }

        // =================================================================
        //  Knopf
        // =================================================================
        public static bool Button(string label, Vector2 size, bool primary = false)
        {
            ImGui.PushID(label);
            var dl = ImGui.GetWindowDrawList();
            Vector2 p = ImGui.GetCursorScreenPos();

            bool pressed = ImGui.InvisibleButton("##b", size);
            bool hovered = ImGui.IsItemHovered();
            bool held    = ImGui.IsItemActive();

            Vector4 fill = primary
                ? (held ? Theme.Shade(Theme.Accent, 0.88f) : (hovered ? Theme.Shade(Theme.Accent, 1.12f) : Theme.Accent))
                : (held ? Theme.Mix(Theme.Frame, Theme.Accent, 0.45f)
                        : (hovered ? Theme.Mix(Theme.Frame, Theme.Accent, 0.30f) : Theme.Frame));

            Vector2 mx = new Vector2(p.X + size.X, p.Y + size.Y);
            dl.AddRectFilled(p, mx, Theme.U32(fill), 7f);
            dl.AddRect(p, mx, Theme.U32(Theme.Line, 0.12f), 7f, 0, 1f);

            Vector4 txt = primary ? Theme.Rgb(0x16, 0x10, 0x22) : Theme.Text;
            Vector2 ts = ImGui.CalcTextSize(label);
            dl.AddText(new Vector2(p.X + (size.X - ts.X) * 0.5f, p.Y + (size.Y - ts.Y) * 0.5f),
                       Theme.U32(txt, 1f), label);

            ImGui.PopID();
            return pressed;
        }

        // =================================================================
        //  Karte
        // =================================================================
        public static void CardBegin(string id, string title)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, Theme.Card);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 10f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 14));
            ImGui.BeginChild(id, new Vector2(0, 0),
                             ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.Borders |
                             ImGuiChildFlags.AlwaysUseWindowPadding);

            if (!string.IsNullOrEmpty(title))
            {
                var dl = ImGui.GetWindowDrawList();
                var font = ImGui.GetFont();
                float fs = ImGui.GetFontSize();
                Vector2 p = ImGui.GetCursorScreenPos();
                float avail = ImGui.GetContentRegionAvail().X;

                dl.AddCircleFilled(new Vector2(p.X + 3f, p.Y + fs * 0.5f), 3f, Theme.U32(Theme.Accent), 10);
                dl.AddText(font, fs * 0.94f, new Vector2(p.X + 14f, p.Y + 1f),
                           Theme.U32(Theme.Text, 0.95f), title);
                float ty = p.Y + fs + 8f;
                dl.AddLine(new Vector2(p.X, ty), new Vector2(p.X + avail, ty), Theme.U32(Theme.Line, 0.07f), 1f);
                ImGui.Dummy(new Vector2(0, fs + 12f));
            }
        }

        public static void CardEnd()
        {
            ImGui.Dummy(new Vector2(0, 2));
            ImGui.EndChild();
            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor();
            ImGui.Dummy(new Vector2(0, 6));
        }

        // =================================================================
        //  Text
        // =================================================================
        public static void Note(string text)
        {
            ImGui.PushTextWrapPos(0f);
            ImGui.TextColored(Theme.TextDim, text);
            ImGui.PopTextWrapPos();
        }

        public static void Separator()
        {
            ImGui.Dummy(new Vector2(0, 3));
            var dl = ImGui.GetWindowDrawList();
            Vector2 p = ImGui.GetCursorScreenPos();
            float avail = ImGui.GetContentRegionAvail().X;
            dl.AddLine(new Vector2(p.X, p.Y), new Vector2(p.X + avail, p.Y), Theme.U32(Theme.Line, 0.10f), 1f);
            ImGui.Dummy(new Vector2(0, 4));
        }

        /// <summary>Aufzaehlungspunkt mit Umbruch - ImGui.BulletText bricht nicht um.</summary>
        public static void Bullet(string text)
        {
            var dl = ImGui.GetWindowDrawList();
            float fs = ImGui.GetFontSize();
            Vector2 p = ImGui.GetCursorScreenPos();
            dl.AddCircleFilled(new Vector2(p.X + 4f, p.Y + fs * 0.5f), 2.5f, Theme.U32(Theme.Accent, 0.9f), 8);
            ImGui.Indent(16f);
            ImGui.PushTextWrapPos(0f);
            ImGui.TextColored(Theme.TextDim, text);
            ImGui.PopTextWrapPos();
            ImGui.Unindent(16f);
            ImGui.Dummy(new Vector2(0, 2));
        }
    }
}
