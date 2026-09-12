# TevvezImGui — Custom Dear ImGui Menu Framework for .NET, without cimgui.dll

A lightweight, reusable immediate-mode menu framework built on **Dear ImGui via ImGui.NET**, rendered with **Silk.NET (OpenGL)**.

Same menu API as `TevvezUI` (WinForms/GDI+ version), but this time **really powered by ImGui** — not just recreated in the same look. Build your config menu once, run it as a standalone window or as a transparent overlay, with almost unchanged code.

No `cimgui.dll`. No native dependency hell. Just NuGet + copy `src`.

![preview](https://github.com/user-attachments/assets/a7d4bbf6-249e-4a28-8ab8-1461e1b18978)

---

## ✨ Features

**Core:**
- True Dear ImGui rendering via `ImGui.NET 1.91.6.1`
- Window + graphics via `Silk.NET.Windowing / Input / OpenGL / OpenGL.Extensions.ImGui`
- Image loading via `StbImageSharp`
- Frameless draggable window with custom header, sidebar, footer
- 1 / 2 / 3 column auto-layout with `ImGui.BeginTable` (shortest-column fill)
- Full theming in one file (`Theme.cs` + `Theme.Apply()`)
- Logo support: `tevvez_logo.png` / `logo.png` as OpenGL texture, fallback vector owl drawn in accent color
- `AllowUnsafeBlocks` only for logo texture + ImGui pointers, no native DLLs

**Widgets (all ImGui-drawn):**
- `Toggle` — switch
- `Slider` / `IntSlider` — float + int sliders with min/max
- `Combo` — selection box
- `ColorPick` — color with alpha (`Vector4 0-1`)
- `Btn` / `Row` — buttons + side-by-side button rows
- `Note` / `Bullet` / `Line` — text, bullet point, separator
- `Card` — grouped panel, `Page` — tab with icon + group header
- `Watermark` — top-edge bar with accent stripe, logo, FPS, clock, custom extra text
- `Overlay.cs` — ESP boxes, radar, FOV circle, crosshair helpers (uses `TevvezOverlay`)

**Framework:**
- 3 ways to build menus: nested `{ }`, flat, or chained — all equivalent
- Data binding via `Bind.cs`: `() => C.MyField` lambdas, no reflection
- `Changed` callback, `Actions()` save/load buttons, `Status()` top-right values, `Footer()`, `StartTab()`, `BeforeDraw()` per-frame hook
- `EnabledWhen = () => ...` conditional enable for any element
- `ConfigStore.cs` — plain-text save/load
- Demo app + `dotnet run -- 2` start-tab argument
- Companion projects:
  - `TevvezImGui/` — standalone window version
  - `TevvezOverlay/` — transparent clickable overlay version (`OverlayApp.cs`, `DemoScene.cs`, `native/`, `publish-aot/`)
  - Same menu code runs in both, only the host (`MenuHost.cs` vs overlay host) differs

**vs TevvezUI:**

| | TevvezUI | TevvezImGui |
|---|---|---|
| Renders with | WinForms + GDI+ | Dear ImGui over OpenGL |
| Dependencies | none | ImGui.NET, Silk.NET x4, StbImageSharp |
| Mode | retained, redraw on change | immediate, redraw every frame |
| Integrate | copy `src` folder | copy folder + 6 NuGet refs |
| Runs on | .NET Framework + .NET | .NET 8+ |
| Colors | `System.Drawing.Color` | `System.Numerics.Vector4 (0-1)` |

> Note: for a menu *inside* exclusive-fullscreen Killing Floor, the C++ DLL hooking DirectX 9 Present is still the right path — ImGui.NET cannot draw into another 32-bit process Present hook. Use this repo for external windows / overlays.

---

## 🚀 Quick Start

### Requirements

- .NET 8 SDK+
- Windows 10/11 x64 (Silk.NET OpenGL)
- No native `cimgui.dll` needed

### 1. Integrate

Copy the `TevvezImGui/src` folder into your project and add to your `.csproj`:

```xml
<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<PackageReference Include="ImGui.NET" Version="1.91.6.1" />
<PackageReference Include="Silk.NET.Windowing" Version="2.22.0" />
<PackageReference Include="Silk.NET.Input" Version="2.22.0" />
<PackageReference Include="Silk.NET.OpenGL" Version="2.22.0" />
<PackageReference Include="Silk.NET.OpenGL.Extensions.ImGui" Version="2.22.0" />
<PackageReference Include="StbImageSharp" Version="2.30.15" />
```

### 2. Minimal example

```csharp
using System.Numerics;

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
                    new Combo    ("Box style", () => C.BoxStyle, "Corners", "Full box"),
                    new ColorPick("Box color", () => C.BoxColor),
                },
            },
        };

        m.Run();
    }
}
```

The other two styles work identically:

```csharp
// flat
m.Tab("Visuals", NavIcon.Eye, "ESP");
m.Card("World");
m.Toggle("WallHack", () => C.WallHack);

// chained
m.Tab("Visuals").Card("World").Toggle("WallHack", () => C.WallHack);
```

### 3. Run demo

```bat
dotnet run --project TevvezImGui.csproj
dotnet run --project TevvezImGui.csproj -- 2
:: second form opens directly on tab index 2
```

For overlay:

```bat
dotnet run --project TevvezOverlay
```

---

## 📁 Project layout

```text
TevvezImGui/
  src/
    Theme.cs       # colors, sizes, full ImGui style
    Bind.cs        # binds widgets to your fields via lambdas
    Ui.cs          # Toggle, Slider, Combo, ColorPick, Button, Card — ImGui-drawn
    Elements.cs    # element classes, Card + Page with column distribution
    Menu.cs        # header, sidebar, footer, builder surface
    MenuHost.cs    # Silk.NET window, OpenGL, render loop, logo texture
    Logo.cs        # logo + icons
    ConfigStore.cs # save/load as text file
    Watermark.cs   # top-edge bar
    Overlay.cs     # ESP, radar, FOV, crosshair — uses TevvezOverlay
  demo/            # example menu
  TevvezImGui.csproj
  tevvez_logo.png

TevvezOverlay/
  OverlayApp.cs    # transparent overlay host
  DemoScene.cs     # demo scene
  Program.cs
  native/          # native helpers if needed
  publish-aot/     # AOT publish profile
```

| File | What it does |
|------|--------------|
| `Theme.cs` | All colors, metrics, `TwoColumnWidth=600`, `ThreeColumnWidth=1080`, `Apply()` |
| `Bind.cs` | `Func<T>` getters/setters for widgets |
| `Ui.cs` | Actual `ImGui.Checkbox/Slider/Combo/ColorEdit` wrappers with Tevvez style |
| `Elements.cs` | `Page`, `Card`, `Row`, layout + shortest-column distribution |
| `Menu.cs` | `Tab()`, `Card()`, `Toggle()`, `Slider()`, `Combo()`, `Color()`, `Button()`, `Text()`, `Bullet()`, `Separator()`, `Actions()`, `Status()`, `Footer()`, `Run(w,h)` |
| `MenuHost.cs` | Creates Silk.NET window (frameless), OpenGL context, ImGui controller, main loop |
| `Watermark.cs` | Drawn bar: accent stripe + logo + label + FPS + extra + time, pushes menu down when visible |
| `ConfigStore.cs` | Text-file persistence |

---

## 🧩 Building blocks

| Nested style | Flat / chained | Purpose |
|---|---|---|
| `new Page("Name", NavIcon.Eye, "GROUP")` | `Tab(...)` | page / tab |
| `new Card("Title")` | `Card(...)` | card panel |
| `new Toggle("Text", () => C.Field)` | `Toggle(...)` | switch |
| `new Slider("Text", () => C.Val, 0, 100)` | `Slider(...)` | float slider |
| `new IntSlider("Text", () => C.Num, 1, 10)` | `SliderInt(...)` | int slider |
| `new Combo("Text", () => C.Idx, "A", "B")` | `Combo(...)` | dropdown |
| `new ColorPick("Text", () => C.Color)` | `Color(...)` | color + alpha |
| `new Btn("Text", Action)` | `Button(...)` | button |
| `new Row(new Btn(..), new Btn(..))` | `ButtonRow(...)` | side-by-side buttons |
| `new Note("Text")` | `Text(...)` | explanatory text |
| `new Bullet("Text")` | `Bullet(...)` | bullet point |
| `new Line()` | `Separator()` | divider |

Colors are `System.Numerics.Vector4` 0-1 here (ImGui native). In `TevvezUI` it was `System.Drawing.Color` — that's the only menu-code difference between the two.

---

## 🖼️ Frame: header, footer, watermark

```csharp
m.Changed = () => Save();                             // after every change
m.Actions("Save config", Save, "Load config", Load);  // bottom-left buttons
m.Status(() => new[] { fps + " FPS", zeds + " zeds" });// top-right values
m.Footer("INSERT opens the menu");
m.StartTab(0);

m.Run(950, 660);   // window size
```

Conditional enable:

```csharp
new Slider("Predict", () => C.Predict, 0, 100) { EnabledWhen = () => C.SilentAim },
```

Watermark (actually drawn, not just config):

```csharp
m.BeforeDraw = () =>
{
    m.Watermark.Show     = C.Watermark;
    m.Watermark.Position = C.WatermarkPos;      // 0 left, 1 center, 2 right
    m.Watermark.ShowFps  = C.WatermarkFps;
    m.Watermark.ShowTime = C.WatermarkTime;
    m.Watermark.Extra    = () => zeds + " zeds"; // any extra, null = hidden
};
```

`BeforeDraw` runs before every frame — set live values there. When enabled, the menu shifts down so both don't overlap.

---

## 🎨 Theming

Everything in `Theme.cs`. After changing a color, call:

```csharp
Theme.Accent = Theme.AccentPresets[3];   // Pink
Theme.Apply();
```

Columns:

```csharp
public const float TwoColumnWidth   = 600f;
public const float ThreeColumnWidth = 1080f;
```

Cards go to the currently shortest column via `ImGui.BeginTable`.

Logo: put `tevvez_logo.png` (or `logo.png`) next to the `.exe` — `MenuHost` loads it as OpenGL texture on start. If missing, `Logo` draws the owl from polygons in the current accent color.

---

## 🖱️ Controls

| Action | Effect |
|---|---|
| Drag header | move window |
| Bottom-right grip | resize |
| Mouse wheel | scroll content |
| ESC | close |

Window is frameless because the menu brings its own header — a Windows title bar would look foreign.

---

## 📦 Dependencies / Credits

- https://github.com/zaafar/ClickableTransparentOverlay — transparent overlay base
- https://github.com/ImGuiNET/ImGui.NET — .NET binding for Dear ImGui
- https://github.com/ocornut/imgui — Dear ImGui itself
- https://github.com/cimgui/cimgui — C API reference (no binary used here)
- Silk.NET + StbImageSharp via NuGet

Game ESP preview (draggable labels), radar window, aim indicator, FOV circle + crosshair *settings* exist here, but the in-game drawing is done by the C++ DLL over Killing Floor — only the watermark is drawn standalone without a game.

---

## 🤝 Contributing

1. Fork, `git checkout -b feat/my-widget`
2. Keep `TevvezUI` API parity where possible so menu code stays portable
3. Test both `TevvezImGui` window + `TevvezOverlay`
4. PR with screenshot

---

## 📄 License

Add your `LICENSE` here (MIT / GPL-3.0 — pick one and keep it consistent with TevvezUI).
