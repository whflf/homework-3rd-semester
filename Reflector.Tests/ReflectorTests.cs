namespace Reflector.Tests;

public class ReflectorTests
{
    [Test]
    public void TestNotClass()
    {
        Assert.Throws<ArgumentException>(() => Reflector.PrintStructure(typeof(TestStruct)));
    }

    [Test]
    public void TestPrintSimpleClass()
    {
        Reflector.PrintStructure(typeof(SimpleClass));
        var actual = File.ReadAllText("SimpleClass.cs");
        var expected = File.ReadAllText("TestSimpleClass.txt");
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestDiffClasses()
    {
        var result = Reflector.DiffClasses(typeof(SimpleClass), typeof(SecondSimpleClass));
        var expected = "Fields differences: number boolean \nMethods differences: ReturnInt ReturnString ";
        Assert.That(result, Is.EqualTo(expected));
    }
}
