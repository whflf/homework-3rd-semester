namespace MYNUnitTestRun.Tests;

using MyNUnitTestRun;
using NUnit.Framework.Internal;
using System.Linq;

public class MYNUnitTestRunTests
{
    private StringWriter consoleOutput;
    private TextWriter originalOutput;
    private string output;

    [SetUp]
    public void SetUp()
    {
        consoleOutput = new StringWriter();
        originalOutput = Console.Out;
        Console.SetOut(consoleOutput);

        TestRun.RunTests("../../../../builds-tests");
        this.output = consoleOutput.ToString();
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(originalOutput);
        consoleOutput.Dispose();
    }

    [Test]
    public void Test_BeforeAndAfterClassMethodsRunOnce()
    {
        string[] methodStrings = ["BEFORE CLASS METHOD", "AFTER CLASS METHOD", "BeforeClass Method Two",
                                    "AfterClass Method Two"];

        for (int i = 0; i < methodStrings.Length; ++i)
        {
            Assert.That(output.Split(methodStrings[i]).Length - 1, Is.EqualTo(1));
        }
    }

    [Test]
    public void Test_BeforeAndAfterMethodsRunOnceForEveryTest()
    {
        string[] methodStrings = ["~ Test beginning ~", "~ Test end ~", "Test began!", "Test ended!"];
        int[] counts = { 4, 4, 3, 3 };

        for (int i = 0; i < methodStrings.Length; ++i)
        {
            Assert.That(output.Split(methodStrings[i]).Length - 1, Is.EqualTo(counts[i]));
        }
    }

    [Test]
    public void Test_IgnoreTestNotRun()
    {
        string[] methodStrings = ["Test IgnoreTest in class Test1 ignored: Wanna sleep ..."];
        for (int i = 0; i < methodStrings.Length; ++i)
        {
            Assert.That(output.Split(methodStrings[i]).Length - 1, Is.EqualTo(1));
        }
    }

    [Test]
    public void Test_SimpleTestsPass()
    {
        string[] methodStrings = [
            "Test VoidTest in class Test1 passed",
            "Test BoolTest in class Test1 passed",
            "Test StringTest of class Test1 result"];

        for (int i = 0; i < methodStrings.Length; ++i)
        {
            Assert.That(output.Split(methodStrings[i]).Length - 1, Is.EqualTo(1));
        }
    }

    [Test]
    public void Test_SimpleTestFail()
    {
        string[] methodStrings = [
            "Test FalseBoolTest in class Test2 failed",
            "Test WrongExceptionTest in class Test2 failed. AbandonedMutexException",
            "Test UnexpectedExceptionTest in class Test2 failed. RankException"];

        for (int i = 0; i < methodStrings.Length; ++i)
        {
            Assert.That(output.Split(methodStrings[i]).Length - 1, Is.EqualTo(1));
        }
    }
}
