namespace Reflector.Tests;

public class SecondSimpleClass
{
    public char character;
    private static string name = "SecondSimpleClass";
    public bool boolean;
    public static void PrintName()
    {
        Console.WriteLine(name);
    }
    private string ReturnString(bool value)
    {
        return value ? "true" : "false";
    }
}
