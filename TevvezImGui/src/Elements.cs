using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using ImGuiNET;

namespace TevvezImGui
{
    /// <summary>Symbol links neben dem Eintrag in der Seitenleiste.</summary>
    public enum NavIcon
    {
        Dot, Eye, Crosshair, Target, List, Palette, Console, Gear, Info, Shield, Bolt
    }

    /// <summary>Basis aller Bedienelemente.</summary>
    public abstract class Element
    {
        public string     Tooltip;
        public Func<bool> EnabledWhen;

        /// <summary>Zeichnet das Element. true = Wert wurde geaendert.</summary>
        protected abstract bool DrawInner();

        internal void Render(Menu menu)
        {
            bool enabled = (EnabledWhen == null) || EnabledWhen();
            if (!enabled) ImGui.BeginDisabled(true);
            bool changed = DrawInner();
            if (!enabled) ImGui.EndDisabled();
            if (changed) menu.Fire();
        }

        /// <summary>Grober Hoehenanteil fuer die Spaltenverteilung.</summary>
        internal virtual float Weight { get { return 1f; } }
    }

    // ---- Schalter -------------------------------------------------------
    public class Toggle : Element
    {
        private readonly string _label;
        private readonly Bind<bool> _value;

        public Toggle(string label, Expression<Func<bool>> value, string tooltip = null)
        {
            _label = label; _value = Bind<bool>.Of(value); Tooltip = tooltip;
        }

        protected override bool DrawInner()
        {
            bool v = _value.Get();
            if (Ui.Toggle(_label, ref v, Tooltip)) { _value.Set(v); return true; }
            return false;
        }
    }

    // ---- Regler ---------------------------------------------------------
    public class Slider : Element
    {
        private readonly string _label, _format, _zero;
        private readonly Bind<float> _value;
        private readonly float _min, _max;

        public Slider(string label, Expression<Func<float>> value, float min, float max,
                      string format = "0.##", string tooltip = null, string zeroText = null)
        {
            _label = label; _value = Bind<float>.Of(value);
            _min = min; _max = max; _format = format; _zero = zeroText;
            Tooltip = tooltip;
        }

        protected override bool DrawInner()
        {
            float v = _value.Get();
            bool done = Ui.Slider(_label, ref v, _min, _max, _format, _zero, Tooltip);
            _value.Set(v);          // waehrend des Ziehens laufend uebernehmen
            return done;            // gemeldet wird erst beim Loslassen
        }
    }

    /// <summary>Regler fuer ganze Zahlen.</summary>
    public class IntSlider : Element
    {
        private readonly string _label;
        private readonly Bind<int> _value;
        private readonly float _min, _max;

        public IntSlider(string label, Expression<Func<int>> value, int min, int max, string tooltip = null)
        {
            _label = label; _value = Bind<int>.Of(value);
            _min = min; _max = max; Tooltip = tooltip;
        }

        protected override bool DrawInner()
        {
            float v = _value.Get();
            bool done = Ui.Slider(_label, ref v, _min, _max, "0", null, Tooltip);
            _value.Set((int)Math.Round(v));
            return done;
        }
    }

    // ---- Auswahlfeld ----------------------------------------------------
    public class Combo : Element
    {
        private readonly string _label;
        private readonly Bind<int> _value;
        private readonly string[] _items;

        public Combo(string label, Expression<Func<int>> value, params string[] items)
        {
            _label = label; _value = Bind<int>.Of(value); _items = items;
        }

        protected override bool DrawInner()
        {
            int v = _value.Get();
            if (Ui.Combo(_label, ref v, _items, Tooltip)) { _value.Set(v); return true; }
            return false;
        }
    }

    // ---- Farbfeld -------------------------------------------------------
    public class ColorPick : Element
    {
        private readonly string _label;
        private readonly Bind<Vector4> _value;

        public ColorPick(string label, Expression<Func<Vector4>> value, string tooltip = null)
        {
            _label = label; _value = Bind<Vector4>.Of(value); Tooltip = tooltip;
        }

        protected override bool DrawInner()
        {
            Vector4 v = _value.Get();
            if (Ui.ColorField(_label, ref v, Tooltip)) { _value.Set(v); return true; }
            return false;
        }
    }

    // ---- Knopf ----------------------------------------------------------
    /// <summary>Heisst bewusst Btn - Button ist in vielen Rahmenwerken schon vergeben.</summary>
    public class Btn : Element
    {
        public string Label;
        public Action Click;
        public bool   Primary;
        public float  Width;

        public Btn(string label, Action click, bool primary = false, float width = 130f)
        {
            Label = label; Click = click; Primary = primary; Width = width;
        }

        protected override bool DrawInner()
        {
            if (Ui.Button(Label, new Vector2(Width, 30f), Primary))
            {
                if (Click != null) Click();
            }
            return false;   // ein Knopf aendert keinen gebundenen Wert
        }
    }

    /// <summary>Mehrere Knoepfe nebeneinander. Passt die Breiten an die Karte an.</summary>
    public class Row : Element
    {
        public readonly Btn[] Buttons;
        public Row(params Btn[] buttons) { Buttons = buttons; }

        protected override bool DrawInner()
        {
            const float gap = 8f;
            float avail = ImGui.GetContentRegionAvail().X;

            float want = 0f;
            foreach (var b in Buttons) want += b.Width;
            want += gap * (Buttons.Length - 1);

            float scale = 1f;
            if (want > avail && Buttons.Length > 0)
            {
                float inner = want - gap * (Buttons.Length - 1);
                if (inner > 0f) scale = (avail - gap * (Buttons.Length - 1)) / inner;
            }

            for (int i = 0; i < Buttons.Length; i++)
            {
                if (i > 0) ImGui.SameLine(0f, gap);
                var b = Buttons[i];
                float w = Math.Max(40f, b.Width * scale);
                if (Ui.Button(b.Label, new Vector2(w, 30f), b.Primary) && b.Click != null) b.Click();
            }
            return false;
        }
    }

    // ---- Text -----------------------------------------------------------
    public class Note : Element
    {
        public string Text;
        public Note(string text) { Text = text; }
        protected override bool DrawInner() { Ui.Note(Text); return false; }
        internal override float Weight { get { return 1.2f; } }
    }

    public class Bullet : Element
    {
        public string Text;
        public Bullet(string text) { Text = text; }
        protected override bool DrawInner() { Ui.Bullet(Text); return false; }
        internal override float Weight { get { return 1.4f; } }
    }

    public class Line : Element
    {
        protected override bool DrawInner() { Ui.Separator(); return false; }
        internal override float Weight { get { return 0.4f; } }
    }

    // =====================================================================
    //  Karte und Seite
    // =====================================================================

    /// <summary>Eine Karte: Titel mit Akzentpunkt, Trennlinie, darunter die Elemente.</summary>
    public class Card : IEnumerable<Element>
    {
        public string Title;
        public readonly List<Element> Elements = new List<Element>();

        /// <summary>Gewicht fuer die Spaltenverteilung. Kleiner 0 = aus dem Inhalt ableiten.</summary>
        public float Weight = -1f;

        private readonly string _id;
        private static int _counter;

        public Card(string title)
        {
            Title = title;
            _id = "##card" + (_counter++);
        }

        public void Add(Element e) { Elements.Add(e); }
        public IEnumerator<Element> GetEnumerator() { return Elements.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator()     { return Elements.GetEnumerator(); }

        internal void Draw(Menu menu)
        {
            Ui.CardBegin(_id, Title);
            foreach (var e in Elements) e.Render(menu);
            Ui.CardEnd();
        }

        internal float AutoWeight
        {
            get
            {
                if (Weight >= 0f) return Weight;
                float w = 2.5f;                       // Titel und Rand
                foreach (var e in Elements) w += e.Weight;
                return w;
            }
        }
    }

    /// <summary>Eine Seite. Enthaelt Karten.</summary>
    public class Page : IEnumerable<Card>
    {
        public string  Name;
        public string  Group;
        public NavIcon Icon;

        public readonly List<Card> Cards = new List<Card>();

        public Page(string name, NavIcon icon = NavIcon.Dot, string group = null)
        {
            Name = name; Icon = icon; Group = group;
        }

        public void Add(Card c) { Cards.Add(c); }
        public IEnumerator<Card> GetEnumerator() { return Cards.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator()  { return Cards.GetEnumerator(); }

        private static int ColumnCount(float width, int cardCount)
        {
            int cols = 1;
            if      (width >= Theme.ThreeColumnWidth) cols = 3;
            else if (width >= Theme.TwoColumnWidth)   cols = 2;
            if (cols > cardCount) cols = cardCount;
            return cols < 1 ? 1 : cols;
        }

        /// <summary>
        /// Karten auf Spalten verteilen. Jede Karte kommt in die bisher
        /// kuerzeste Spalte, damit unten keine grosse Luecke stehen bleibt.
        /// </summary>
        internal void Draw(Menu menu)
        {
            int n = Cards.Count;
            if (n == 0) return;

            int cols = ColumnCount(ImGui.GetContentRegionAvail().X, n);
            if (cols <= 1)
            {
                foreach (var c in Cards) c.Draw(menu);
                return;
            }

            var assign = new int[n];
            var load   = new float[cols];
            for (int i = 0; i < n; i++)
            {
                int best = 0;
                for (int c = 1; c < cols; c++) if (load[c] < load[best] - 0.01f) best = c;
                assign[i] = best;
                load[best] += Cards[i].AutoWeight;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(12, 0));
            if (ImGui.BeginTable("##cols" + Name, cols, ImGuiTableFlags.SizingStretchSame))
            {
                ImGui.TableNextRow();
                for (int c = 0; c < cols; c++)
                {
                    ImGui.TableSetColumnIndex(c);
                    for (int i = 0; i < n; i++) if (assign[i] == c) Cards[i].Draw(menu);
                }
                ImGui.EndTable();
            }
            ImGui.PopStyleVar();
        }
    }
}
