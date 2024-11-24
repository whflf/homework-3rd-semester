using MyNUnit;

namespace test1;

[TestClass]
public class Test1
{
    [BeforeClass]
    public static void BeforeClassPrint()
    {
        Console.WriteLine("     BEFORE CLASS METHOD");
    }

    [AfterClass]
    public static void AfterClassPrint()
    {
        Console.WriteLine("     AFTER CLASS METHOD");
    }

    [Before]
    public void BeforePrint()
    {
        Console.WriteLine("     ~ Test beginning ~");
    }

    [After]
    public void AfterPrint()
    {
        Console.WriteLine("     ~ Test end ~");
    }

    [Test]
    public void VoidTest()
    {
        Console.WriteLine("     > Meow !!!");
    }

    [Test]
    public bool BoolTest() 
    {
        return true;
    }

    [Test]
    public string StringTest()
    {
        return "purr~";
    }

    [Test(Ignore = "Wanna sleep ...")]
    public int IgnoreTest()
    {
        return -1;
    }

    [Test(Expected = typeof(IOException))]
    public int ExceptionTest()
    {
        throw new IOException();
    }
}
