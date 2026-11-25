using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public sealed class FactAttribute : Attribute { }

public static class Check
{
    public static void True(bool c, string msg = "")
    {
        if (!c) throw new Exception("expected true. " + msg);
    }

    public static void False(bool c, string msg = "")
    {
        if (c) throw new Exception("expected false. " + msg);
    }

    public static void Eq<T>(T expected, T actual, string msg = "")
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"expected {expected}, got {actual}. {msg}");
    }

    public static void Eq(double expected, double actual, int digits, string msg = "")
    {
        double tol = 0.5 * Math.Pow(10, -digits);
        if (Math.Abs(expected - actual) > tol)
            throw new Exception($"expected {expected}, got {actual}. {msg}");
    }

    public static void InRange(double v, double lo, double hi, string msg = "")
    {
        if (v < lo || v > hi) throw new Exception($"{v} not in [{lo},{hi}]. {msg}");
    }

    public static void Throws<T>(Action a) where T : Exception
    {
        try { a(); }
        catch (T) { return; }
        throw new Exception($"expected {typeof(T).Name} to be thrown");
    }
}

public static class TestRunner
{
    public static int Main()
    {
        int pass = 0, fail = 0;
        var methods = Assembly.GetExecutingAssembly().GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null)
            .OrderBy(m => m.DeclaringType.Name).ThenBy(m => m.Name);

        foreach (var m in methods)
        {
            try
            {
                object instance = m.IsStatic ? null : Activator.CreateInstance(m.DeclaringType);
                m.Invoke(instance, null);
                pass++;
                Console.WriteLine($"PASS {m.DeclaringType.Name}.{m.Name}");
            }
            catch (TargetInvocationException ex)
            {
                fail++;
                Console.WriteLine($"FAIL {m.DeclaringType.Name}.{m.Name}: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                fail++;
                Console.WriteLine($"FAIL {m.DeclaringType.Name}.{m.Name}: {ex.Message}");
            }
        }

        Console.WriteLine($"\nResult: {pass} passed, {fail} failed");
        return fail == 0 ? 0 : 1;
    }
}
