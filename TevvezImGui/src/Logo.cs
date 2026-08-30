using System;
using System.Numerics;
using ImGuiNET;

namespace TevvezImGui
{
    /// <summary>
    /// Das Logo. Ist eine Textur gesetzt, wird sie benutzt; sonst wird die Eule
    /// aus Polygonen gezeichnet - gleiche Form, gleiche Farben, ohne Datei.
    ///
    /// Textur setzen (der Host macht das, wenn eine Bilddatei daneben liegt):
    ///     Logo.Texture = (nint)glTextureId;
    ///     Logo.TextureSize = new Vector2(breite, hoehe);
    /// </summary>
    public static class Logo
    {
        public static nint    Texture;
        public static Vector2 TextureSize;

        /// <summary>Ausschnitt des Kopfes im Bild - fuer kleine Stellen.</summary>
        public static Vector2 HeadUv0 = new Vector2(0.36f, 0.00f);
        public static Vector2 HeadUv1 = new Vector2(0.65f, 0.44f);

        /// <summary>Ganzes Logo, mittig in die Box, Seitenverhaeltnis bleibt.</summary>
        public static void Draw(ImDrawListPtr dl, Vector2 boxMin, Vector2 boxMax, bool wings = true)
        {
            float bw = boxMax.X - boxMin.X, bh = boxMax.Y - boxMin.Y;
            if (bw <= 1f || bh <= 1f) return;
            Vector2 c = new Vector2((boxMin.X + boxMax.X) * 0.5f, (boxMin.Y + boxMax.Y) * 0.5f);

            if (Texture != 0 && TextureSize.X > 0 && TextureSize.Y > 0)
            {
                float sc = Math.Min(bw / TextureSize.X, bh / TextureSize.Y);
                Vector2 half = TextureSize * sc * 0.5f;
                dl.AddImage(Texture, c - half, c + half);
                return;
            }

            float uw = wings ? 6.20f : 1.28f;
            float uh = wings ? 3.00f : 1.70f;
            float offY = wings ? 0.16f : -0.15f;
            float s = Math.Min(bw / uw, bh / uh);
            DrawOwl(dl, new Vector2(c.X, c.Y - offY * s), s, wings);
        }

        /// <summary>Nur der Kopf. Fluegel und Schriftzug werden klein zu Matsch.</summary>
        public static void DrawHead(ImDrawListPtr dl, Vector2 boxMin, Vector2 boxMax)
        {
            float bw = boxMax.X - boxMin.X, bh = boxMax.Y - boxMin.Y;
            if (bw <= 1f || bh <= 1f) return;
            Vector2 c = new Vector2((boxMin.X + boxMax.X) * 0.5f, (boxMin.Y + boxMax.Y) * 0.5f);

            if (Texture != 0 && TextureSize.X > 0 && TextureSize.Y > 0)
            {
                float srcW = (HeadUv1.X - HeadUv0.X) * TextureSize.X;
                float srcH = (HeadUv1.Y - HeadUv0.Y) * TextureSize.Y;
                float sc = Math.Min(bw / srcW, bh / srcH);
                Vector2 half = new Vector2(srcW, srcH) * sc * 0.5f;
                dl.AddImage(Texture, c - half, c + half, HeadUv0, HeadUv1);
                return;
            }

            Draw(dl, boxMin, boxMax, false);
        }

        // Fuenf Federlagen je Seite, nach aussen laenger werdend
        private static readonly float[][] Wing =
        {
            new[] { -0.86f, -0.56f, -3.10f, -1.34f, -2.96f, -1.08f, -0.90f, -0.36f },
            new[] { -0.80f, -0.34f, -2.62f, -0.90f, -2.34f, -0.58f, -0.84f, -0.12f },
            new[] { -0.82f, -0.08f, -2.26f, -0.30f, -1.98f,  0.02f, -0.86f,  0.16f },
            new[] { -0.86f,  0.20f, -1.90f,  0.24f, -1.62f,  0.56f, -0.90f,  0.44f },
            new[] { -0.90f,  0.48f, -1.54f,  0.66f, -1.28f,  0.98f, -0.94f,  0.72f },
        };

        private static readonly float[] Head =
        {
            -0.62f, -1.00f, -0.30f, -0.60f,  0.00f, -0.32f,  0.30f, -0.60f,
             0.62f, -1.00f,  0.46f, -0.44f,  0.64f, -0.02f,  0.48f,  0.38f,
             0.00f,  0.70f, -0.48f,  0.38f, -0.64f, -0.02f, -0.46f, -0.44f,
        };

        private static void DrawOwl(ImDrawListPtr dl, Vector2 c, float s, bool wings)
        {
            Func<float, float, Vector2> P = (ox, oy) => new Vector2(c.X + ox * s, c.Y + oy * s);

            uint deep = Theme.U32(Theme.Shade(Theme.Accent, 0.42f), 1f);
            uint mid  = Theme.U32(Theme.Shade(Theme.Accent, 0.72f), 1f);
            uint edge = Theme.U32(Theme.Accent, 1f);
            uint eye  = Theme.U32(Theme.Rgb(0xF5, 0xDE, 0xFF), 1f);
            uint glow = Theme.U32(Theme.Rgb(0xC7, 0x66, 0xFF), 0.45f);

            if (wings)
            {
                uint[] wc = { edge, mid, deep, mid, edge };
                for (int i = 0; i < Wing.Length; i++)
                {
                    var w = Wing[i];
                    var L = new[] { P(w[0], w[1]), P(w[2], w[3]), P(w[4], w[5]), P(w[6], w[7]) };
                    var R = new[] { P(-w[0], w[1]), P(-w[2], w[3]), P(-w[4], w[5]), P(-w[6], w[7]) };
                    AddPoly(dl, L, wc[i]);
                    AddPoly(dl, R, wc[i]);
                }

                AddPoly(dl, new[] { P(-0.54f, 0.84f), P(-0.18f, 0.78f), P(0.42f, 1.60f), P(0.06f, 1.66f) }, mid);
                AddPoly(dl, new[] { P( 0.54f, 0.84f), P( 0.18f, 0.78f), P(-0.42f, 1.60f), P(-0.06f, 1.66f) }, edge);
            }

            var head = new Vector2[Head.Length / 2];
            for (int i = 0; i < head.Length; i++) head[i] = P(Head[i * 2], Head[i * 2 + 1]);
            AddPoly(dl, head, deep);
            for (int i = 0; i < head.Length; i++)
                dl.AddLine(head[i], head[(i + 1) % head.Length], edge, Math.Max(1f, s * 0.085f));

            AddPoly(dl, new[] { P(-0.50f, -0.30f), P(-0.04f, -0.06f), P(-0.48f, 0.02f) }, mid);
            AddPoly(dl, new[] { P( 0.50f, -0.30f), P( 0.04f, -0.06f), P( 0.48f, 0.02f) }, mid);

            dl.AddCircleFilled(P(-0.30f, 0.06f), s * 0.30f, glow, 16);
            dl.AddCircleFilled(P( 0.30f, 0.06f), s * 0.30f, glow, 16);
            AddPoly(dl, new[] { P(-0.46f, -0.06f), P(-0.12f, 0.06f), P(-0.14f, 0.20f), P(-0.46f, 0.14f) }, eye);
            AddPoly(dl, new[] { P( 0.46f, -0.06f), P( 0.12f, 0.06f), P( 0.14f, 0.20f), P( 0.46f, 0.14f) }, eye);

            AddPoly(dl, new[] { P(-0.15f, 0.16f), P(0.15f, 0.16f), P(0.00f, 0.62f) }, edge);
        }

        /// <summary>
        /// Gefuellte Flaeche. Der Kopf ist konkav (der Einschnitt zwischen den
        /// Federohren), deshalb ueber den Pfad und nicht als konvexes Polygon.
        /// </summary>
        private static void AddPoly(ImDrawListPtr dl, Vector2[] pts, uint col)
        {
            foreach (var p in pts) dl.PathLineTo(p);
            dl.PathFillConcave(col);
        }

        /// <summary>Kleine Symbole der Seitenleiste - keine Symbolschrift noetig.</summary>
        public static void NavIcon(ImDrawListPtr dl, TevvezImGui.NavIcon icon, Vector2 c, float s, uint col)
        {
            switch (icon)
            {
                case TevvezImGui.NavIcon.Eye:
                    dl.PathArcTo(new Vector2(c.X, c.Y + s * 0.55f), s * 1.15f, -2.5f, -0.65f, 16); dl.PathStroke(col, 0, 1.5f);
                    dl.PathArcTo(new Vector2(c.X, c.Y - s * 0.55f), s * 1.15f, 0.65f, 2.5f, 16);  dl.PathStroke(col, 0, 1.5f);
                    dl.AddCircleFilled(c, s * 0.34f, col, 10);
                    break;
                case TevvezImGui.NavIcon.Crosshair:
                    dl.AddLine(new Vector2(c.X - s, c.Y), new Vector2(c.X - s * 0.35f, c.Y), col, 1.5f);
                    dl.AddLine(new Vector2(c.X + s * 0.35f, c.Y), new Vector2(c.X + s, c.Y), col, 1.5f);
                    dl.AddLine(new Vector2(c.X, c.Y - s), new Vector2(c.X, c.Y - s * 0.35f), col, 1.5f);
                    dl.AddLine(new Vector2(c.X, c.Y + s * 0.35f), new Vector2(c.X, c.Y + s), col, 1.5f);
                    break;
                case TevvezImGui.NavIcon.Target:
                    dl.AddCircle(c, s, col, 16, 1.5f);
                    dl.AddCircleFilled(c, s * 0.32f, col, 10);
                    break;
                case TevvezImGui.NavIcon.List:
                    for (int i = -1; i <= 1; i++)
                        dl.AddLine(new Vector2(c.X - s, c.Y + i * s * 0.55f),
                                   new Vector2(c.X + s, c.Y + i * s * 0.55f), col, 1.5f);
                    break;
                case TevvezImGui.NavIcon.Palette:
                    dl.AddCircle(c, s, col, 16, 1.5f);
                    dl.AddCircleFilled(new Vector2(c.X + s * 0.02f, c.Y - s * 0.45f), s * 0.2f, col, 8);
                    break;
                case TevvezImGui.NavIcon.Console:
                    dl.AddRect(new Vector2(c.X - s, c.Y - s * 0.75f), new Vector2(c.X + s, c.Y + s * 0.75f), col, 2f, 0, 1.5f);
                    dl.AddLine(new Vector2(c.X - s * 0.5f, c.Y - s * 0.25f), new Vector2(c.X - s * 0.1f, c.Y), col, 1.5f);
                    dl.AddLine(new Vector2(c.X - s * 0.1f, c.Y), new Vector2(c.X - s * 0.5f, c.Y + s * 0.25f), col, 1.5f);
                    break;
                case TevvezImGui.NavIcon.Gear:
                    for (int i = 0; i < 6; i++)
                    {
                        double a = i * Math.PI / 3.0;
                        dl.AddLine(new Vector2(c.X + (float)Math.Cos(a) * s * 0.45f, c.Y + (float)Math.Sin(a) * s * 0.45f),
                                   new Vector2(c.X + (float)Math.Cos(a) * s,          c.Y + (float)Math.Sin(a) * s), col, 1.5f);
                    }
                    dl.AddCircle(c, s * 0.42f, col, 12, 1.5f);
                    break;
                case TevvezImGui.NavIcon.Info:
                    dl.AddCircle(c, s, col, 16, 1.5f);
                    dl.AddLine(new Vector2(c.X, c.Y - s * 0.1f), new Vector2(c.X, c.Y + s * 0.5f), col, 1.5f);
                    dl.AddCircleFilled(new Vector2(c.X, c.Y - s * 0.5f), 1.3f, col, 6);
                    break;
                case TevvezImGui.NavIcon.Shield:
                    dl.AddQuad(new Vector2(c.X, c.Y - s), new Vector2(c.X + s * 0.8f, c.Y - s * 0.45f),
                               new Vector2(c.X, c.Y + s), new Vector2(c.X - s * 0.8f, c.Y - s * 0.45f), col, 1.5f);
                    break;
                case TevvezImGui.NavIcon.Bolt:
                    dl.PathLineTo(new Vector2(c.X + s * 0.3f, c.Y - s));
                    dl.PathLineTo(new Vector2(c.X - s * 0.5f, c.Y + s * 0.15f));
                    dl.PathLineTo(new Vector2(c.X, c.Y + s * 0.15f));
                    dl.PathLineTo(new Vector2(c.X - s * 0.3f, c.Y + s));
                    dl.PathLineTo(new Vector2(c.X + s * 0.5f, c.Y - s * 0.15f));
                    dl.PathLineTo(new Vector2(c.X, c.Y - s * 0.15f));
                    dl.PathFillConcave(col);
                    break;
                default:
                    dl.AddCircleFilled(c, 3f, col, 8);
                    break;
            }
        }
    }
}
