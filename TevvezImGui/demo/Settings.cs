using System.Numerics;

namespace TevvezImGui.Demo
{
    /// <summary>
    /// Beispiel-Einstellungen. Ganz normale oeffentliche Felder - mehr braucht
    /// weder das Menue noch das Speichern.
    /// </summary>
    public class Settings
    {
        // Visuals
        public bool  WallHack     = true;
        public bool  NoShadows    = false;
        public bool  RemoveFog    = false;
        public bool  Crosshair    = true;
        public bool  PlayersEsp   = false;

        public bool  BoxEsp       = true;
        public bool  Box3D        = false;
        public bool  Skeleton     = true;
        public bool  HealthBar    = true;
        public bool  HealthText   = true;
        public bool  DistanceEsp  = true;
        public bool  NameEsp      = true;

        public int   BoxStyle     = 0;     // 0 = Ecken, 1 = Rechteck
        public int   Snaplines    = 1;
        public float BoxThickness = 2f;
        public float TextSize     = 1f;
        public float MaxDistance  = 0f;
        public bool  DistanceFade = true;

        // Radar
        public bool  Radar        = true;
        public float RadarSize    = 100f;
        public bool  RadarSimple  = false;
        public bool  RadarSquare  = false;
        public bool  RadarSweep   = true;
        public float RadarBg      = 0.7f;

        // Aimbot
        public bool  Aimbot       = true;
        public bool  AutoAimbot   = false;
        public bool  AimWarning   = false;
        public bool  AutoShot     = false;
        public float AutoShotWait = 250f;
        public bool  AimFov       = true;
        public float AimFovSize   = 300f;
        public bool  SmoothAim    = true;
        public float SmoothSpeed  = 250f;

        public bool  SilentAim    = false;
        public float SilentLead   = 0f;
        public bool  SilentBody   = false;
        public bool  MagicBullet  = false;
        public bool  AimLock      = false;
        public bool  BigHead      = false;
        public float BigHeadSize  = 300f;
        public bool  NoRecoil     = false;
        public bool  NoSpread     = false;

        // Hotkeys
        public int   AimKey       = 1;     // Index in der Auswahl

        // Farben
        public Vector4 BoxVisible = new Vector4(168f/255f, 85f/255f, 247f/255f, 255f/255f);
        public Vector4 BoxHidden = new Vector4(74f/255f, 46f/255f, 143f/255f, 255f/255f);
        public Vector4 SkeletonCol = new Vector4(199f/255f, 156f/255f, 255f/255f, 217f/255f);
        public Vector4 NameCol = new Vector4(239f/255f, 232f/255f, 250f/255f, 255f/255f);
        public Vector4 FovCol = new Vector4(199f/255f, 125f/255f, 255f/255f, 179f/255f);
        public Vector4 CrosshairCol = new Vector4(224f/255f, 170f/255f, 255f/255f, 255f/255f);
        public Vector4 RadarFrame = new Vector4(168f/255f, 118f/255f, 247f/255f, 140f/255f);

        // Watermark
        public bool  Watermark     = true;
        public int   WatermarkPos  = 0;
        public bool  WatermarkFps  = true;
        public bool  WatermarkZeds = true;
        public bool  WatermarkTime = true;
    }
}
