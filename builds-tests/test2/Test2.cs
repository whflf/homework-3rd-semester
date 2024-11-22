namespace test2;

using MyNUnit;

[TestClass]
public class Test2
{
    [BeforeClass]
    public static void BeforeClass()
    {
        Console.WriteLine("BeforeClass Method Two");
    }

    [AfterClass]
    public static void AfterClass()
    {
        Console.WriteLine("AfterClass Method Two");
    }

    [Before]
    public void Before()
    {
        Console.WriteLine("Test began!");
    }

    [After]
    public void After()
    {
        Console.WriteLine("Test ended!");
    }

    [Test]
    public int UnexpectedExceptionTest()
    {
        throw new RankException();
    }

    [Test]
    public bool FalseBoolTest()
    {
        return false;
    }

    [Test(Expected = typeof(ArithmeticException))]
    public void WrongExceptionTest()
    {
        throw new AbandonedMutexException();
    }
}
