using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using ImGuiNET;

namespace TevvezImGui
{
    /// <summary>
    /// Das Menue. Sammelt Seiten und Karten und zeichnet sie jeden Frame mit ImGui.
    ///
    /// Drei Schreibweisen, alle gleichwertig:
    ///
    ///   Klammern:   new Menu(..) { new Page(..) { new Card(..) { new Toggle(..) } } }
    ///   flach:      m.Tab(..); m.Card(..); m.Toggle(..);
    ///   angehaengt: m.Tab(..).Card(..).Toggle(..)
    /// </summary>
    public sealed class Menu : IEnumerable<Page>
    {
        private readonly List<Page> _pages = new List<Page>();
        private Page _page;
        private Card _card;

        public string Title      = "TEVVEZ CHEATS";
        public string Version    = "v1063";
        public string Subtitle   = "Killing Floor  -  UE2  -  DirectX 9";
        public string FooterHint = "INSERT  toggles the menu";

        public int ActiveTab;

        /// <summary>Kurze Werte oben rechts, z.B. () =&gt; new[]{ "60 FPS", "12 on" }.</summary>
        public Func<string[]> StatusChips;

        /// <summary>Wird nach jeder Aenderung aufgerufen - praktisch zum Speichern.</summary>
        public Action Changed;

        /// <summary>Setzt der Host: verschiebt das Fenster, schliesst es.</summary>
        public Action<Vector2> OnDragWindow;
        public Action<Vector2> OnResizeWindow;
        public Action OnClose;

        /// <summary>Der Balken oben am Bildschirmrand.</summary>
        public readonly Watermark Watermark = new Watermark();

        /// <summary>Wird vor jedem Zeichnen aufgerufen - hier laufende Werte setzen.</summary>
        public Action BeforeDraw;

        /// <summary>
        /// true = das Menue ist ein frei bewegliches Fenster innerhalb der
        /// Zeichenflaeche (Overlay). false = es fuellt das ganze Fenster.
        /// </summary>
        public bool Floating;

        private Vector2 _floatPos, _floatSize;
        private bool    _floatInit;

        private Btn _primary, _secondary;
        private readonly Dictionary<int, float> _navAnim = new Dictionary<int, float>();

        public Menu(string title = "TEVVEZ CHEATS", string version = "v1063", string subtitle = null)
        {
            Title = title;
            Version = version;
            if (subtitle != null) Subtitle = subtitle;
        }

        internal void Fire() { if (Changed != null) Changed(); }

        // =================================================================
        //  Aufbau
        // =================================================================
        public void Add(Page page) { _pages.Add(page); _page = page; _card = null; }
        public IEnumerator<Page> GetEnumerator() { return _pages.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator()  { return _pages.GetEnumerator(); }

        public Menu Tab(string name, NavIcon icon = NavIcon.Dot, string group = null)
        {
            _page = new Page(name, icon, group);
            _pages.Add(_page);
            _card = null;
            return this;
        }

        public Menu Card(string title)
        {
            if (_page == null) Tab("Menu");
            _card = new Card(title);
            _page.Cards.Add(_card);
            return this;
        }

        private Card Target { get { if (_card == null) Card(""); return _card; } }

        private Menu Add(Element e) { Target.Elements.Add(e); return this; }

        public Menu Toggle(string label, Expression<Func<bool>> value, string tooltip = null)
        { return Add(new Toggle(label, value, tooltip)); }

        public Menu Slider(string label, Expression<Func<float>> value, float min, float max,
                           string format = "0.##", string tooltip = null)
        { return Add(new Slider(label, value, min, max, format, tooltip)); }

        public Menu SliderInt(string label, Expression<Func<int>> value, int min, int max, string tooltip = null)
        { return Add(new IntSlider(label, value, min, max, tooltip)); }

        public Menu Combo(string label, Expression<Func<int>> value, params string[] items)
        { return Add(new Combo(label, value, items)); }

        public Menu Color(string label, Expression<Func<Vector4>> value, string tooltip = null)
        { return Add(new ColorPick(label, value, tooltip)); }

        public Menu Button(string label, Action click, bool primary = false, float width = 130f)
        { return Add(new Btn(label, click, primary, width)); }

        public Menu ButtonRow(params Btn[] buttons) { return Add(new Row(buttons)); }

        public Menu Text(string text)   { return Add(new Note(text)); }
        public Menu Bullet(string text) { return Add(new Bullet(text)); }
        public Menu Separator()         { return Add(new Line()); }

        /// <summary>Das zuletzt angemeldete Element nur bedienbar, wenn die Bedingung stimmt.</summary>
        public Menu EnabledIf(Func<bool> condition)
        {
            var list = Target.Elements;
            if (list.Count > 0) list[list.Count - 1].EnabledWhen = condition;
            return this;
        }

        public Menu Actions(string primaryLabel, Action primary,
                            string secondaryLabel = null, Action secondary = null)
        {
            _primary = new Btn(primaryLabel, primary, true);
            if (secondaryLabel != null) _secondary = new Btn(secondaryLabel, secondary);
            return this;
        }

        public Menu Status(Func<string[]> chips) { StatusChips = chips; return this; }
        public Menu Footer(string hint)          { FooterHint = hint; return this; }
        public Menu StartTab(int index)          { ActiveTab = index; return this; }

        // =================================================================
        //  Zeichnen
        // =================================================================

        /// <summary>
        /// Laufende Werte uebernehmen. Einmal je Bild aufrufen, vor dem Zeichnen.
        /// Bewusst getrennt: im Overlay wird das Menue nicht jedes Bild gezeichnet,
        /// das Watermark aber schon.
        /// </summary>
        public void Update()
        {
            if (BeforeDraw != null) BeforeDraw();
        }

        /// <summary>Das Watermark auf die Vordergrundliste zeichnen.</summary>
        public void DrawWatermark(Vector2 screenPos, float screenWidth)
        {
            Watermark.Draw(ImGui.GetForegroundDrawList(), screenPos, screenWidth);
        }

        /// <summary>Zeichnet das Menuefenster in die angegebene Flaeche.</summary>
        public void Draw(Vector2 pos, Vector2 size)
        {
            if (Floating)
            {
                if (!_floatInit) { _floatPos = pos; _floatSize = size; _floatInit = true; }
                ImGui.SetNextWindowPos(_floatPos);
                ImGui.SetNextWindowSize(_floatSize);
            }
            else
            {
                ImGui.SetNextWindowPos(pos);
                ImGui.SetNextWindowSize(size);
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            bool open = ImGui.Begin("##tevvez",
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.PopStyleVar();
            if (!open) { ImGui.End(); return; }

            float W = ImGui.GetWindowWidth();
            float H = ImGui.GetWindowHeight();
            float bodyH = H - Theme.HeaderHeight - Theme.FooterHeight;

            DrawHeader(W);
            DrawSidebar(bodyH);

            ImGui.SetCursorPos(new Vector2(Theme.SidebarWidth, Theme.HeaderHeight));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(18, 16));
            ImGui.BeginChild("##content", new Vector2(W - Theme.SidebarWidth, bodyH),
                             ImGuiChildFlags.AlwaysUseWindowPadding);
            if (ActiveTab >= 0 && ActiveTab < _pages.Count) _pages[ActiveTab].Draw(this);
            ImGui.EndChild();
            ImGui.PopStyleVar();

            DrawFooter(W, H);
            ImGui.End();
        }

        private void DrawHeader(float W)
        {
            var dl = ImGui.GetWindowDrawList();
            Vector2 wp = ImGui.GetWindowPos();
            Vector2 a  = wp;
            Vector2 b  = new Vector2(wp.X + W, wp.Y + Theme.HeaderHeight);

            dl.AddRectFilled(a, b, Theme.U32(Theme.Mix(Theme.Panel, Theme.Accent, 0.05f)), 12f,
                             ImDrawFlags.RoundCornersTop);
            dl.AddRectFilledMultiColor(new Vector2(a.X, b.Y - 2f), b,
                Theme.U32(Theme.Accent, 0f), Theme.U32(Theme.Accent, 0.85f),
                Theme.U32(Theme.Accent, 0.85f), Theme.U32(Theme.Accent, 0f));

            // Ziehflaeche: Kopfzeile ohne den Schliessen-Knopf
            ImGui.SetCursorPos(new Vector2(0, 0));
            ImGui.InvisibleButton("##titlebar", new Vector2(Math.Max(1f, W - 46f), Theme.HeaderHeight));
            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                Vector2 d = ImGui.GetIO().MouseDelta;
                if (Floating) _floatPos += d;
                else if (OnDragWindow != null) OnDragWindow(d);
            }

            Logo.DrawHead(dl, new Vector2(a.X + 14f, a.Y + 6f),
                              new Vector2(a.X + 60f, a.Y + Theme.HeaderHeight - 6f));

            var font = ImGui.GetFont();
            float fs = ImGui.GetFontSize();
            float tx = a.X + 72f;

            dl.AddText(font, fs * 1.22f, new Vector2(tx, a.Y + 9f), Theme.U32(Theme.Text), Title);
            float tw = font.CalcTextSizeA(fs * 1.22f, float.MaxValue, 0f, Title).X;
            dl.AddText(font, fs * 0.85f, new Vector2(tx + tw + 8f, a.Y + 15f), Theme.U32(Theme.Accent), Version);
            dl.AddText(font, fs * 0.88f, new Vector2(tx, a.Y + 32f), Theme.U32(Theme.TextDim), Subtitle);

            // Statuswerte rechts
            if (StatusChips != null)
            {
                string[] chips = null;
                try { chips = StatusChips(); } catch { }
                if (chips != null)
                {
                    float x = b.X - 46f;
                    for (int i = chips.Length - 1; i >= 0; i--)
                    {
                        string s = chips[i];
                        if (string.IsNullOrEmpty(s)) continue;
                        Vector2 cs = font.CalcTextSizeA(fs * 0.82f, float.MaxValue, 0f, s);
                        float cw = cs.X + 18f, ch = fs + 8f;
                        Vector2 c0 = new Vector2(x - cw, a.Y + (Theme.HeaderHeight - ch) * 0.5f);
                        Vector2 c1 = new Vector2(x, c0.Y + ch);
                        dl.AddRectFilled(c0, c1, Theme.U32(Theme.Bg, 0.55f), ch * 0.5f);
                        dl.AddRect(c0, c1, Theme.U32(Theme.Line, 0.10f), ch * 0.5f, 0, 1f);
                        dl.AddText(font, fs * 0.82f, new Vector2(c0.X + 9f, c0.Y + 4f),
                                   Theme.U32(i == chips.Length - 1 ? Theme.Accent : Theme.TextDim, 1f), s);
                        x = c0.X - 7f;
                    }
                }
            }

            // Schliessen
            ImGui.SetCursorPos(new Vector2(W - 40f, Theme.HeaderHeight * 0.5f - 13f));
            bool closed = ImGui.InvisibleButton("##close", new Vector2(26, 26));
            bool ch2 = ImGui.IsItemHovered();
            Vector2 cmn = ImGui.GetItemRectMin(), cmx = ImGui.GetItemRectMax();
            if (ch2) dl.AddRectFilled(cmn, cmx, Theme.U32(Theme.Line, 0.10f), 6f);
            uint xc = Theme.U32(ch2 ? Theme.Text : Theme.TextDim, 1f);
            dl.AddLine(new Vector2(cmn.X + 8, cmn.Y + 8), new Vector2(cmx.X - 8, cmx.Y - 8), xc, 1.6f);
            dl.AddLine(new Vector2(cmx.X - 8, cmn.Y + 8), new Vector2(cmn.X + 8, cmx.Y - 8), xc, 1.6f);
            if (closed && OnClose != null) OnClose();
        }

        private void DrawSidebar(float bodyH)
        {
            ImGui.SetCursorPos(new Vector2(0, Theme.HeaderHeight));
            ImGui.PushStyleColor(ImGuiCol.ChildBg, Theme.Panel);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 14));
            ImGui.BeginChild("##sidebar", new Vector2(Theme.SidebarWidth, bodyH),
                             ImGuiChildFlags.AlwaysUseWindowPadding);

            var dl = ImGui.GetWindowDrawList();
            string lastGroup = null;

            for (int i = 0; i < _pages.Count; i++)
            {
                var p = _pages[i];
                if (!string.IsNullOrEmpty(p.Group) && p.Group != lastGroup)
                {
                    if (lastGroup != null) ImGui.Dummy(new Vector2(0, 5));
                    ImGui.Indent(6f);
                    ImGui.TextColored(Theme.A(Theme.TextDim, 0.75f), p.Group.ToUpperInvariant());
                    ImGui.Unindent(6f);
                    ImGui.Dummy(new Vector2(0, 2));
                    lastGroup = p.Group;
                }

                float h = ImGui.GetFrameHeight() + 6f;
                float w = ImGui.GetContentRegionAvail().X;
                Vector2 pos = ImGui.GetCursorScreenPos();

                ImGui.PushID(i);
                bool pressed = ImGui.InvisibleButton("##nav", new Vector2(w, h));
                bool hovered = ImGui.IsItemHovered();
                ImGui.PopID();
                if (pressed) ActiveTab = i;

                float target = (i == ActiveTab) ? 1f : (hovered ? 0.45f : 0f);
                float t;
                if (!_navAnim.TryGetValue(i, out t)) t = target;
                float dt = Math.Min(0.1f, ImGui.GetIO().DeltaTime);
                t += (target - t) * Math.Min(1f, dt * 12f);
                _navAnim[i] = t;

                if (t > 0.01f)
                {
                    Vector2 mx = new Vector2(pos.X + w, pos.Y + h);
                    dl.AddRectFilledMultiColor(pos, mx,
                        Theme.U32(Theme.Accent, 0.22f * t), Theme.U32(Theme.Accent, 0.02f * t),
                        Theme.U32(Theme.Accent, 0.02f * t), Theme.U32(Theme.Accent, 0.22f * t));
                    dl.AddRectFilled(new Vector2(pos.X, pos.Y + 5f), new Vector2(pos.X + 3f, mx.Y - 5f),
                                     Theme.U32(Theme.Accent, t), 2f);
                }

                uint ic = Theme.U32(Theme.Mix(Theme.TextDim, Theme.Text, t), 1f);
                Logo.NavIcon(dl, p.Icon, new Vector2(pos.X + 22f, pos.Y + h * 0.5f), 7f, ic);
                float fs = ImGui.GetFontSize();
                dl.AddText(new Vector2(pos.X + 38f, pos.Y + (h - fs) * 0.5f),
                           Theme.U32(Theme.Mix(Theme.TextDim, Theme.Text, t)), p.Name);
            }

            // Knoepfe unten festhalten
            if (_primary != null || _secondary != null)
            {
                float btnH = 34f;
                float need = (_primary != null ? btnH + 6f : 0f) + (_secondary != null ? btnH : 0f);
                float y = ImGui.GetWindowHeight() - 14f - need;
                if (y > ImGui.GetCursorPosY()) ImGui.SetCursorPosY(y);

                float w = ImGui.GetContentRegionAvail().X;
                if (_primary != null)
                {
                    if (Ui.Button(_primary.Label, new Vector2(w, btnH), true) && _primary.Click != null)
                        _primary.Click();
                    ImGui.Dummy(new Vector2(0, 6));
                }
                if (_secondary != null)
                {
                    if (Ui.Button(_secondary.Label, new Vector2(w, btnH), false) && _secondary.Click != null)
                        _secondary.Click();
                }
            }

            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
        }

        private void DrawFooter(float W, float H)
        {
            var dl = ImGui.GetWindowDrawList();
            Vector2 wp = ImGui.GetWindowPos();
            Vector2 a = new Vector2(wp.X, wp.Y + H - Theme.FooterHeight);
            Vector2 b = new Vector2(wp.X + W, wp.Y + H);

            dl.AddRectFilled(a, b, Theme.U32(Theme.Panel), 12f, ImDrawFlags.RoundCornersBottom);
            dl.AddLine(a, new Vector2(b.X, a.Y), Theme.U32(Theme.Line, 0.06f), 1f);

            float fs = ImGui.GetFontSize();
            dl.AddCircleFilled(new Vector2(a.X + 16f, a.Y + Theme.FooterHeight * 0.5f), 3f,
                               Theme.U32(Theme.Accent), 8);
            dl.AddText(new Vector2(a.X + 28f, a.Y + (Theme.FooterHeight - fs) * 0.5f),
                       Theme.U32(Theme.TextDim), FooterHint);

            // Griff unten rechts zum Groessenaendern
            ImGui.SetCursorPos(new Vector2(W - 18f, H - 18f));
            ImGui.InvisibleButton("##grip", new Vector2(16, 16));
            bool gh = ImGui.IsItemHovered() || ImGui.IsItemActive();
            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                Vector2 d = ImGui.GetIO().MouseDelta;
                if (Floating)
                {
                    _floatSize.X = Math.Max(660f, _floatSize.X + d.X);
                    _floatSize.Y = Math.Max(470f, _floatSize.Y + d.Y);
                }
                else if (OnResizeWindow != null) OnResizeWindow(d);
            }
            uint gc = Theme.U32(Theme.Line, gh ? 0.45f : 0.22f);
            for (int i = 0; i < 3; i++)
            {
                float o = 4f + i * 4f;
                dl.AddLine(new Vector2(b.X - o, b.Y - 4f), new Vector2(b.X - 4f, b.Y - o), gc, 1.4f);
            }
        }
    }
}
