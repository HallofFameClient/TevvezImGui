using System;
using System.Numerics;
using ImGuiNET;

namespace TevvezImGui.OverlayDemo
{
    /// <summary>
    /// Testdaten fuer das Overlay: ein paar bewegte Gegner und Radarpunkte,
    /// damit man ESP, Radar und FOV auch ohne Spiel sieht.
    ///
    /// Im Ernstfall ersetzt du diese Klasse durch deine echten Daten - die
    /// Szene selbst bleibt unveraendert.
    /// </summary>
    public static class DemoScene
    {
        private static readonly string[] Names =
        { "Clot", "Gorefast", "Bloat", "Crawler", "Stalker", "Husk", "Siren", "Scrake", "Flesh Pound" };

        private static readonly float[] Phase  = { 0.0f, 0.8f, 1.7f, 2.4f, 3.1f, 3.9f, 4.6f, 5.3f, 6.0f };
        private static readonly float[] Health = { 100, 220, 300, 80, 120, 480, 260, 1000, 1500 };

        public static void Update(OverlayScene scene)
        {
            float t = (float)ImGui.GetTime();
            Vector2 screen = ImGui.GetIO().DisplaySize;
            Vector2 center = screen * 0.5f;

            scene.Entities.Clear();
            scene.Blips.Clear();

            for (int i = 0; i < Names.Length; i++)
            {
                float p = Phase[i];

                // gemaechliche Kreisbahn um die Bildmitte, jede Bahn etwas anders
                float radius = 180f + i * 55f;
                float speed  = 0.18f + i * 0.03f;
                float a = t * speed + p;

                float x = center.X + (float)Math.Cos(a) * radius;
                float y = center.Y + (float)Math.Sin(a * 0.7f) * (radius * 0.32f) + 30f;

                // je weiter weg, desto kleiner - wie eine echte Projektion
                float dist = 8f + i * 9f + (float)Math.Sin(a) * 4f;
                float h = 190f * (26f / (26f + dist));
                float w = h * 0.42f;

                float hp = Health[i] * (0.35f + 0.65f * (0.5f + 0.5f * (float)Math.Sin(t * 0.6f + p)));

                scene.Entities.Add(new EspEntry
                {
                    BoxMin    = new Vector2(x - w * 0.5f, y - h * 0.5f),
                    BoxMax    = new Vector2(x + w * 0.5f, y + h * 0.5f),
                    Name      = Names[i],
                    Health    = hp,
                    HealthMax = Health[i],
                    Distance  = dist,
                    Visible   = (i % 3) != 1,
                });

                float rr = 20f + i * 7f;
                scene.Blips.Add(new RadarBlip
                {
                    Offset = new Vector2((float)Math.Cos(a) * rr, (float)Math.Sin(a) * rr),
                    Kind   = i >= 7 ? 2 : (i >= 5 ? 1 : 0),
                });
            }
        }
    }
}
