namespace Reflector.Tests;

internal class SimpleClass
{
    public char character;
    private static string name = "SimpleClass";
    public int number;

    public static void PrintName()
    {
        Console.WriteLine(name);
    }

    private int ReturnInt(bool value)
    {
        return value ? 1 : 0;
    }
}
