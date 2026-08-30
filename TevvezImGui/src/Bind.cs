using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TevvezImGui
{
    /// <summary>
    /// Verbindung zwischen einem Bedienelement und einem Feld deiner Einstellungen.
    ///
    /// Statt Lesen und Schreiben einzeln anzugeben, schreibst du nur
    ///     () =&gt; cfg.WallHack
    /// Daraus werden Getter und Setter selbst gebaut. Das haelt den Aufruf kurz und
    /// haelt beim Umbenennen im Editor mit - anders als ein Feldname als Text.
    /// </summary>
    public sealed class Bind<T>
    {
        public Func<T>   Get { get; private set; }
        public Action<T> Set { get; private set; }

        /// <summary>Name des Feldes - wird zum Speichern in der Konfiguration benutzt.</summary>
        public string Key { get; private set; }

        private Bind() { }

        public static Bind<T> Of(Func<T> get, Action<T> set, string key = null)
        {
            return new Bind<T> { Get = get, Set = set, Key = key ?? "" };
        }

        /// <summary>Aus einem Ausdruck wie () =&gt; cfg.WallHack.</summary>
        public static Bind<T> Of(Expression<Func<T>> expr)
        {
            var member = expr.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(
                    "Der Ausdruck muss direkt auf ein Feld oder eine Eigenschaft zeigen, z.B. () => cfg.WallHack");

            Func<T> getter = expr.Compile();

            // Zielobjekt einmal ermitteln (bei () => cfg.X ist das cfg)
            object target = null;
            if (member.Expression != null)
                target = Expression.Lambda(member.Expression).Compile().DynamicInvoke();

            Action<T> setter;
            var field = member.Member as FieldInfo;
            var prop  = member.Member as PropertyInfo;
            if (field != null)      setter = v => field.SetValue(target, v);
            else if (prop != null)  setter = v => prop.SetValue(target, v, null);
            else throw new ArgumentException("Nur Felder und Eigenschaften koennen gebunden werden.");

            return new Bind<T> { Get = getter, Set = setter, Key = member.Member.Name };
        }
    }
}
