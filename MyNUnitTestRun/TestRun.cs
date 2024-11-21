using MyNUnit;
using System.Diagnostics;
using System.Reflection;

namespace MyNUnitTestRun;

public static class TestRun
{
    public static void RunTests(string directory)
    {
        var assemblyNames = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);
        var assemblies = new List<Assembly>();

        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                assemblies.Add(Assembly.LoadFrom(assemblyName));
            }
            catch (FileLoadException)
            {
                continue;
            }
        }

        foreach (var assembly in assemblies)
        {
            Console.WriteLine(assembly.FullName + ":");

            var testClasses = assembly.GetTypes().Where(t => t.IsClass && t.GetCustomAttributes<TestClassAttribute>().Any());

            foreach (var testClass in testClasses)
            {
                var beforeClassMethods = testClass.GetMethods().Where(m => m.GetCustomAttributes<BeforeClassAttribute>().Any());
                foreach (var beforeClassMethod in beforeClassMethods)
                {
                    if (!beforeClassMethod.IsStatic)
                    {
                        throw new InvalidOperationException(
                            $"  Method {beforeClassMethod.Name} in {testClass.Name} with [BeforeClass] must be static.");
                    }
                    beforeClassMethod.Invoke(null, null);
                }

                var beforeMethods = testClass.GetMethods().Where(m => m.GetCustomAttributes<BeforeAttribute>().Any());
                var afterMethods = testClass.GetMethods().Where(m => m.GetCustomAttributes<AfterAttribute>().Any());

                var testMethods = testClass.GetMethods().Where(m => m.GetCustomAttributes<TestAttribute>().Any());
                var tasks = new List<Task>();
                foreach (var testMethod in testMethods)
                {
                    tasks.Add(Task.Run(() => HandleTestMethod(testMethod, testClass, beforeMethods.ToArray(), afterMethods.ToArray())));
                }
                Task.WhenAll(tasks).Wait();

                var afterClassMethods = testClass.GetMethods().Where(m => m.GetCustomAttributes<AfterClassAttribute>().Any());
                foreach (var afterClassMethod in afterClassMethods)
                {
                    if (!afterClassMethod.IsStatic)
                    {
                        throw new InvalidOperationException(
                            $"  Method {afterClassMethod.Name} in {testClass.Name} with [AfterClass] must be static.");
                    }
                    afterClassMethod.Invoke(null, null);
                }
            }
        }
    }

    private static void HandleTestMethod(MethodInfo testMethod, Type testClass, MethodInfo[] beforeMethods, MethodInfo[] afterMethods)
    {
        var testAttribute = testMethod.GetCustomAttribute<TestAttribute>();
        if (!string.IsNullOrEmpty(testAttribute.Ignore))
        {
            Console.WriteLine($"    Test {testMethod.Name} in class {testClass.Name} ignored: {testAttribute.Ignore}");
            return;
        }

        var testMethodInstance = Activator.CreateInstance(testClass);

        foreach (var beforeMethod in beforeMethods)
        {
            beforeMethod.Invoke(testMethodInstance, null);
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var expectedException = testAttribute.Expected;
        try
        {
            object? result = null;
            var returnType = testMethod.ReturnType;

            if (returnType == typeof(void))
            {
                testMethod.Invoke(testMethodInstance, null);
            }
            else
            {
                result = testMethod.Invoke(testMethodInstance, null);
            }
            stopwatch.Stop();

            if (expectedException is not null)
            {
                Console.WriteLine(
                    $"    Test {testMethod.Name} in class {testClass.Name} failed. " +
                    $"Expected exception {expectedException.Name} was not thrown.\n" +
                    $"        {stopwatch.ElapsedMilliseconds} ms");
            }
            else
            {
                if (returnType == typeof(void) || returnType == typeof(bool) && (bool?)result is true)
                {
                    Console.WriteLine($"    Test {testMethod.Name} in class {testClass.Name} passed.\n" +
                        $"        {stopwatch.ElapsedMilliseconds} ms");
                }
                else if (returnType == typeof(bool?) && (bool?)result == false)
                {
                    Console.WriteLine($"    Test {testMethod.Name} in class {testClass.Name} failed.\n" +
                        $"        {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Console.WriteLine($"    Test {testMethod.Name} of class {testClass.Name} result: {result}\n" +
                        $"        {stopwatch.ElapsedMilliseconds} ms");
                }
            }
        }
        catch (Exception ex)
        {
            var actualException = ex.InnerException ?? ex;
            if (expectedException is not null && actualException.GetType() == expectedException)
            {
                Console.WriteLine($"    Test {testMethod.Name} in class {testClass.Name} passed.\n" +
                    $"        {stopwatch.ElapsedMilliseconds} ms");
            }
            else
            {
                Console.WriteLine($"    Test {testMethod.Name} in class {testClass.Name} failed. " +
                    $"{actualException.GetType().Name}: {actualException.Message}\n" +
                    $"        {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        foreach (var afterMethod in afterMethods)
        {
            afterMethod.Invoke(testMethodInstance, null);
        }

        Console.WriteLine($"        {stopwatch.ElapsedMilliseconds} ms");
    }
}
