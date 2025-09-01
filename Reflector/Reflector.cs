namespace Reflector;

using System.Reflection;
using System.Text;

public static class Reflector
{
    public static void PrintStructure(Type someClass, int nestLevel=0)
    {
        if (!someClass.IsClass)
        {
            throw new ArgumentException("Argument is not class");
        }

        var fileName = $"{someClass.Name}.cs";
        var tabs = string.Concat(Enumerable.Repeat("    ", nestLevel));

        using var writer = new StreamWriter(fileName);

        var classModifiers = new StringBuilder(someClass.IsPublic ? "public" : "internal");
        if (someClass.IsAbstract && someClass.IsSealed)
        {
            classModifiers.Append(" static");
        }
        else if (someClass.IsAbstract)
        {
            classModifiers.Append(" abstract");
        }
        else if (someClass.IsSealed)
        {
            classModifiers.Append(" sealed");
        }

        var className = someClass.Name;
        if (someClass.IsGenericType)
        {
            var genericArgs = string.Join(", ", someClass.GetGenericArguments().Select(arg => arg.Name));
            className = $"{someClass.Name.Split('`')[0]}<{genericArgs}>";
        }

        writer.WriteLine($"{tabs}{classModifiers} class {className}");

        if (someClass.BaseType is not null && someClass.BaseType != typeof(object))
        {
            var baseType = someClass.BaseType.Name;
            if (someClass.BaseType.IsGenericType)
            {
                var genericArgs = string.Join(", ", someClass.BaseType.GetGenericArguments().Select(arg => arg.Name));
                baseType = $"{someClass.BaseType.Name.Split('`')[0]}<{genericArgs}>";
            }
            writer.WriteLine($"{tabs}    : {baseType}");
        }
        else if (someClass.GetInterfaces().Length > 0)
        {
            var interfaces = string.Join(", ", someClass.GetInterfaces().Select(i => i.Name));
            writer.WriteLine($"{tabs}    : {interfaces}");
        }

        writer.WriteLine($"{tabs}" + "{");

        foreach (var field in someClass.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        {
            var visibility = GetVisibility(field);
            var isStatic = field.IsStatic ? "static " : "";
            writer.WriteLine($"{tabs}    {visibility} {isStatic}{field.FieldType.Name} {field.Name};");
        }

        foreach (var method in someClass.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
        {
            var visibility = GetVisibility(method);
            var isStatic = method.IsStatic ? "static " : "";
            var returnType = method.ReturnType.Name;

            var methodName = method.Name;
            if (method.IsGenericMethod)
            {
                var genericArgs = string.Join(", ", method.GetGenericArguments().Select(arg => arg.Name));
                methodName += $"<{genericArgs}>";
            }

            var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));

            writer.WriteLine($"{tabs}    {visibility} {isStatic}{returnType} {methodName}({parameters});");
        }

        foreach (var nestedType in someClass.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            try
            {
                PrintStructure(nestedType, nestLevel + 1);
            }
            catch (ArgumentException)
            {
                continue;
            }
        }
        writer.WriteLine($"{tabs}" + "}");
    }

    public static string DiffClasses(Type a, Type b)
    {
        if (!a.IsClass || !b.IsClass)
        {
            throw new ArgumentException("Arguments are not classes");
        }

        var fieldsA = a.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var fieldsB = b.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        var diffFields = fieldsA.Except(fieldsB, new MemberInfoComparer())
                                .Concat(fieldsB.Except(fieldsA, new MemberInfoComparer()));

        var diffMembers = new StringBuilder("Fields differences: ");
        foreach (var field in diffFields)
        {
            diffMembers.Append(field.Name + " ");
        }

        var methodsA = a.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var methodsB = b.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        var diffMethods = methodsA.Except(methodsB, new MemberInfoComparer())
                                  .Concat(methodsB.Except(methodsA, new MemberInfoComparer()));

        diffMembers.Append("\nMethods differences: ");
        foreach (var method in diffMethods)
        {
            diffMembers.Append(method.Name + " ");
        }

        return diffMembers.ToString();
    }

    private static string GetVisibility(MemberInfo member)
    {
        if (member is FieldInfo field)
        {
            if (field.IsPublic) return "public";
            if (field.IsPrivate) return "private";
            if (field.IsFamily) return "protected";
            if (field.IsAssembly) return "internal";
            if (field.IsFamilyOrAssembly) return "protected internal";
        }
        else if (member is MethodBase method)
        {
            if (method.IsPublic) return "public";
            if (method.IsPrivate) return "private";
            if (method.IsFamily) return "protected";
            if (method.IsAssembly) return "internal";
            if (method.IsFamilyOrAssembly) return "protected internal";
        }

        return "private"; 
    }

    private class MemberInfoComparer : IEqualityComparer<MemberInfo>
    {
        public bool Equals(MemberInfo? x, MemberInfo? y)
        {
            return x?.Name == y?.Name && x?.MemberType == y?.MemberType;
        }

        public int GetHashCode(MemberInfo obj)
        {
            return obj.Name.GetHashCode() ^ obj.MemberType.GetHashCode();
        }
    }
}
