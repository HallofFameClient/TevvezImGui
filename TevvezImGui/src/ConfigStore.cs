using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TevvezImGui
{
    /// <summary>
    /// Speichert und laedt ein Einstellungsobjekt als einfache Textdatei
    /// (Name=Wert, eine Zeile je Feld). Keine Bibliothek noetig, die Datei
    /// laesst sich im Editor lesen und von Hand aendern.
    ///
    ///     ConfigStore.Save(cfg, "settings.cfg");
    ///     ConfigStore.Load(cfg, "settings.cfg");
    /// </summary>
    public static class ConfigStore
    {
        public static void Save<[DynamicallyAccessedMembers(Kept)] T>(T settings, string path)
        {
            if (settings == null) return;
            var lines = new List<string>();
            foreach (var m in Members(typeof(T)))
            {
                object v = Read(settings, m);
                if (v == null) continue;
                lines.Add(m.Name + "=" + ToText(v));
            }
            File.WriteAllLines(path, lines.ToArray());
        }

        public static void Load<[DynamicallyAccessedMembers(Kept)] T>(T settings, string path)
        {
            if (settings == null || !File.Exists(path)) return;

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '#' || line[0] == ';') continue;
                int i = line.IndexOf('=');
                if (i <= 0) continue;
                map[line.Substring(0, i).Trim()] = line.Substring(i + 1).Trim();
            }

            foreach (var m in Members(typeof(T)))
            {
                string text;
                if (!map.TryGetValue(m.Name, out text)) continue;
                object v = FromText(text, TypeOf(m));
                if (v != null) Write(settings, m, v);
            }
        }

        // ---- innen -------------------------------------------------------
        /// <summary>
        /// Was der Trimmer stehen lassen muss. Ohne diese Anmerkung wirft
        /// NativeAOT die Felder weg und GetFields() liefert eine leere Liste -
        /// das Speichern liefe dann still ins Leere.
        /// </summary>
        private const DynamicallyAccessedMemberTypes Kept =
            DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

        private static IEnumerable<MemberInfo> Members(
            [DynamicallyAccessedMembers(Kept)] Type t)
        {
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (Supported(f.FieldType)) yield return f;
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (p.CanRead && p.CanWrite && Supported(p.PropertyType)) yield return p;
        }

        private static bool Supported(Type t)
        {
            return t == typeof(bool) || t == typeof(int) || t == typeof(float) ||
                   t == typeof(double) || t == typeof(string) || t == typeof(Vector4);
        }

        private static Type TypeOf(MemberInfo m)
        {
            var f = m as FieldInfo;
            return f != null ? f.FieldType : ((PropertyInfo)m).PropertyType;
        }
        private static object Read(object o, MemberInfo m)
        {
            var f = m as FieldInfo;
            return f != null ? f.GetValue(o) : ((PropertyInfo)m).GetValue(o, null);
        }
        private static void Write(object o, MemberInfo m, object v)
        {
            var f = m as FieldInfo;
            if (f != null) f.SetValue(o, v); else ((PropertyInfo)m).SetValue(o, v, null);
        }

        private static string ToText(object v)
        {
            if (v is Vector4)
            {
                var c = (Vector4)v;
                return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", c.X, c.Y, c.Z, c.W);
            }
            if (v is bool)   return ((bool)v) ? "1" : "0";
            if (v is float)  return ((float)v).ToString("R", CultureInfo.InvariantCulture);
            if (v is double) return ((double)v).ToString("R", CultureInfo.InvariantCulture);
            return Convert.ToString(v, CultureInfo.InvariantCulture);
        }

        private static object FromText(string s, Type t)
        {
            try
            {
                if (t == typeof(Vector4))
                {
                    var p = s.Split(',');
                    if (p.Length != 4) return null;
                    return new Vector4(
                        float.Parse(p[0], CultureInfo.InvariantCulture),
                        float.Parse(p[1], CultureInfo.InvariantCulture),
                        float.Parse(p[2], CultureInfo.InvariantCulture),
                        float.Parse(p[3], CultureInfo.InvariantCulture));
                }
                if (t == typeof(bool))   return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (t == typeof(int))    return int.Parse(s, CultureInfo.InvariantCulture);
                if (t == typeof(float))  return float.Parse(s, CultureInfo.InvariantCulture);
                if (t == typeof(double)) return double.Parse(s, CultureInfo.InvariantCulture);
                if (t == typeof(string)) return s;
            }
            catch { }
            return null;
        }
    }
}
