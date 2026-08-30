# TevvezImGui

Dasselbe Menü wie in `TevvezUI` — aber diesmal **wirklich mit ImGui**
(ImGui.NET), nicht nur im gleichen Aussehen nachgebaut.

Fenster und Grafik kommen von Silk.NET (OpenGL), gezeichnet wird alles von
Dear ImGui. Die Aufbau-Oberfläche ist absichtlich identisch zu `TevvezUI`, du
kannst deinen Menü-Code also fast unverändert zwischen beiden hin- und
herschieben.

---

## Unterschied zu TevvezUI

| | TevvezUI | TevvezImGui |
|---|---|---|
| Zeichnet mit | WinForms + GDI+ | Dear ImGui über OpenGL |
| Abhängigkeiten | keine | ImGui.NET, Silk.NET, StbImageSharp |
| Modus | retained, nur bei Änderung neu | immediate, jeden Frame neu |
| Einbauen | `src`-Ordner kopieren | Ordner kopieren **und** vier NuGet-Pakete |
| Läuft mit | .NET Framework und .NET | .NET 8+ |

Beide sind eigenständige Fenster. Ein **durchsichtiges Overlay** über dem
Bildschirm liegt daneben in `TevvezOverlay` — gleiches Menü, gleicher Quellcode,
nur ein anderer Rahmen.

Für ein Menü *innerhalb* von Killing Floor im exklusiven Vollbild bleibt die
C++-DLL der richtige Weg — ImGui.NET kann nicht in den DirectX-9-Present-Hook
eines fremden 32-Bit-Prozesses zeichnen.

---

## Einbauen

Ordner `src` kopieren und diese Pakete in die `.csproj`:

```xml
<PackageReference Include="ImGui.NET" Version="1.91.6.1" />
<PackageReference Include="Silk.NET.Windowing" Version="2.22.0" />
<PackageReference Include="Silk.NET.Input" Version="2.22.0" />
<PackageReference Include="Silk.NET.OpenGL" Version="2.22.0" />
<PackageReference Include="Silk.NET.OpenGL.Extensions.ImGui" Version="2.22.0" />
<PackageReference Include="StbImageSharp" Version="2.30.15" />
```

Dazu `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` — die Textur des Logos und
der ImGui-Zeiger brauchen das.

| Datei | Inhalt |
|---|---|
| `Theme.cs` | Farben, Maße, kompletter ImGui-Stil |
| `Bind.cs` | verbindet Bedienelemente mit deinen Feldern |
| `Ui.cs` | Schalter, Regler, Auswahl, Farbfeld, Knopf, Karte — direkt mit ImGui gezeichnet |
| `Elements.cs` | die Element-Klassen, Karte und Seite mit Spaltenverteilung |
| `Menu.cs` | Kopfzeile, Seitenleiste, Fußzeile und die Aufbau-Oberfläche |
| `MenuHost.cs` | Fenster, OpenGL, Renderschleife, Logo-Textur |
| `Logo.cs` | Logo und Symbole |
| `ConfigStore.cs` | Speichern/Laden als Textdatei |
| `Watermark.cs` | der Balken oben am Rand |
| `Overlay.cs` | ESP, Radar, FOV und Fadenkreuz — benutzt `TevvezOverlay` |

---

## Das kürzeste vollständige Beispiel

```csharp
class Cfg
{
    public bool    WallHack  = true;
    public float   RadarSize = 100f;
    public int     BoxStyle  = 0;
    public Vector4 BoxColor  = new Vector4(0.66f, 0.33f, 0.97f, 1f);
}

static class Program
{
    static readonly Cfg C = new Cfg();

    static void Main()
    {
        var m = new Menu("TEVVEZ CHEATS", "v1.0")
        {
            new Page("Visuals", NavIcon.Eye, "ESP")
            {
                new Card("World")
                {
                    new Toggle   ("WallHack",  () => C.WallHack),
                    new Slider   ("Radar",     () => C.RadarSize, 50, 300),
                    new Combo    ("Box style", () => C.BoxStyle, "Ecken", "Rechteck"),
                    new ColorPick("Boxfarbe",  () => C.BoxColor),
                },
            },
        };

        m.Run();
    }
}
```

Die beiden anderen Schreibweisen gehen genauso:

```csharp
// flach
m.Tab("Visuals", NavIcon.Eye, "ESP");
m.Card("World");
m.Toggle("WallHack", () => C.WallHack);

// angehängt
m.Tab("Visuals").Card("World").Toggle("WallHack", () => C.WallHack);
```

---

## Die Bausteine

| Klammer-Schreibweise | Flach / angehängt | Wofür |
|---|---|---|
| `new Page("Name", NavIcon.Eye, "GRUPPE")` | `Tab(...)` | Seite |
| `new Card("Titel")` | `Card(...)` | Karte |
| `new Toggle("Text", () => C.Feld)` | `Toggle(...)` | Schalter |
| `new Slider("Text", () => C.Wert, 0, 100)` | `Slider(...)` | Regler |
| `new IntSlider("Text", () => C.Zahl, 1, 10)` | `SliderInt(...)` | Regler für ganze Zahlen |
| `new Combo("Text", () => C.Index, "A", "B")` | `Combo(...)` | Auswahlfeld |
| `new ColorPick("Text", () => C.Farbe)` | `Color(...)` | Farbfeld mit Alpha |
| `new Btn("Text", Machwas)` | `Button(...)` | Knopf |
| `new Row(new Btn(..), new Btn(..))` | `ButtonRow(...)` | Knöpfe nebeneinander |
| `new Note("Text")` | `Text(...)` | erklärender Text |
| `new Bullet("Text")` | `Bullet(...)` | Aufzählungspunkt |
| `new Line()` | `Separator()` | Trennlinie |

Farben sind hier `System.Numerics.Vector4` mit Werten von 0 bis 1 — so erwartet
ImGui sie. In `TevvezUI` war es `System.Drawing.Color`; das ist der einzige
Unterschied im Menü-Code zwischen beiden Fassungen.

---

## Der Rahmen

```csharp
m.Changed = () => Save();                                // nach jeder Änderung
m.Actions("Save config", Save, "Load config", Load);     // Knöpfe unten links
m.Status(() => new[] { fps + " FPS", zeds + " zeds" });  // Werte oben rechts
m.Footer("INSERT öffnet das Menü");
m.StartTab(0);

m.Run(950, 660);   // Fenstergröße
```

Ein Element nur bedienbar, wenn eine Bedingung stimmt:

```csharp
new Slider("Predict", () => C.Predict, 0, 100) { EnabledWhen = () => C.SilentAim },
```

---

## Watermark

Der Balken oben am Rand wird wirklich gezeichnet, nicht nur eingestellt:
Akzentstreifen links, Logo, Beschriftung, dann FPS, freier Zusatz und Uhrzeit.

```csharp
m.BeforeDraw = () =>
{
    m.Watermark.Show     = C.Watermark;
    m.Watermark.Position = C.WatermarkPos;      // 0 links, 1 mitte, 2 rechts
    m.Watermark.ShowFps  = C.WatermarkFps;
    m.Watermark.ShowTime = C.WatermarkTime;
    m.Watermark.Extra    = () => zeds + " zeds";   // beliebiger Zusatz, null = weg
};
```

`BeforeDraw` läuft vor jedem Bild - dort setzt du laufende Werte. Ist das
Watermark an, rückt das Menü nach unten, damit sich beide nicht überlagern.

---

## Was hier nicht dabei ist

Das Menü ist vollständig, die **Spielgrafik** aber nicht - dafür gibt es hier
kein Spiel:

- ESP-Vorschau mit ziehbaren Beschriftungen
- Radar-Fenster
- Aim-Ziel-Anzeige
- FOV-Kreis und Fadenkreuz

Diese Teile zeichnet die C++-DLL über das Bild von Killing Floor. Die
Einstellungen dafür sind hier alle vorhanden, gezeichnet wird von ihnen nur das
Watermark - das ist das einzige Element, das auch ohne Spiel Sinn ergibt.

---

## Spalten

Karten werden je nach Fensterbreite auf **eine, zwei oder drei Spalten**
verteilt, jede kommt in die bisher kürzeste Spalte. Umgesetzt mit
`ImGui.BeginTable`. Grenzen stehen in `Theme.cs`:

```csharp
public const float TwoColumnWidth   = 600f;
public const float ThreeColumnWidth = 1080f;
```

---

## Aussehen ändern

Alles in `Theme.cs`. Nach einer Farbänderung `Theme.Apply()` aufrufen, damit der
ImGui-Stil neu gesetzt wird:

```csharp
Theme.Accent = Theme.AccentPresets[3];   // Pink
Theme.Apply();
```

---

## Logo

Liegt `tevvez_logo.png` (oder `logo.png`) neben der `.exe`, lädt `MenuHost` es
beim Start als OpenGL-Textur. Fehlt die Datei, zeichnet `Logo` die Eule aus
Polygonen in der aktuellen Akzentfarbe — beides sieht gleich aus, nur die
Auflösung unterscheidet sich.

---

## Beispielprogramm

```bash
dotnet run --project TevvezImGui.csproj
```

Mit einer Zahl startet es direkt auf der gewünschten Seite:

```bash
dotnet run --project TevvezImGui.csproj -- 2
```

---

## Bedienung

| Aktion | Wirkung |
|---|---|
| Kopfzeile ziehen | Fenster verschieben |
| Griff unten rechts | Größe ändern |
| Mausrad | Inhalt scrollen |
| ESC | schließen |

Das Fenster ist rahmenlos, weil das Menü seine eigene Kopfzeile mitbringt — eine
Windows-Titelleiste darüber sähe fremd aus.
