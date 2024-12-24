// <copyright file="MatrixMultiplicationTests.cs" company="Elena Makarova">
// Copyright (c) Elena Makarova. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Tests;

/// <summary>
/// Contains unit tests for validating the correctness of matrix multiplication methods
/// from the <see cref="MatrixMultiplication"/> class using both parallel and sequential approaches.
/// </summary>
/// <remarks>
/// The tests compare the results of parallel and sequential matrix multiplication
/// for various matrix sizes to ensure they produce the same results.
/// </remarks>
public class MatrixMultiplicationTests
{
    private static readonly object[] TestCases =
    {
        new object[] { "./TestFiles/test500x500.txt", "./TestFiles/test500x500.txt" },
        new object[] { "./TestFiles/test1000x500.txt", "./TestFiles/test500x500.txt" },
        new object[] { "./TestFiles/test1000x1500.txt", "./TestFiles/test1500x1500.txt" },
        new object[] { "./TestFiles/test2000x2500.txt", "./TestFiles/test2500x1500.txt" },
        new object[] { "./TestFiles/test3000x3000.txt", "./TestFiles/test3000x1500.txt" },
        new object[] { "./TestFiles/test1x1.txt", "./TestFiles/test500x500.txt" },
        new object[] { "./TestFiles/test2x3.txt", "./TestFiles/test3x4.txt" },
    };

    private static readonly object[] ArgumentExceptionTestCases =
    {
        new object[] { "./TestFiles/test3000x3000.txt", "./TestFiles/test0x0.txt" },
        new object[] { "./TestFiles/test2x3.txt", "./TestFiles/test3000x1500.txt" },
    };

    private static readonly object[] HardcodedTestCases =
        {
            new object[]
            {
                new int[,]
                {
                    { 1, 2 },
                    { 3, 4 },
                },
                new int[,]
                { { 5, 6 }, { 7, 8 } },
            },
            new object[]
            {
                new int[,]
                {
                    { 2, 3, 4 },
                    { 5, 6, 7 },
                },
                new int[,]
                {
                    { 1, 0 },
                    { 0, 1 },
                    { 1, 1 },
                },
            },
            new object[]
            {
                new int[,]
                {
                    { 1, 2, 3 },
                    { 4, 5, 6 },
                },
                new int[,]
                {
                    { 7, 8 },
                    { 9, 10 },
                    { 11, 12 },
                },
            },
            new object[]
            {
                new int[,]
                {
                    { 2, 3 },
                    { 1, 4 },
                },
                new int[,]
                {
                    { 1, 2 },
                    { 3, 4 },
                },
            },
        };

    /// <summary>
    /// Tests matrix multiplication using both parallel and sequential methods,
    /// and verifies that the results are identical for all test cases.
    /// </summary>
    /// <param name="firstMatrixPath">The file path to the first matrix input.</param>
    /// <param name="secondMatrixPath">The file path to the second matrix input.</param>
    [TestCaseSource(nameof(TestCases))]
    public void TestMatrixMultiplication(string firstMatrixPath, string secondMatrixPath)
    {
        var firstMatrix = FileParser.ReadMatrixFromFile(firstMatrixPath);
        var secondMatrix = FileParser.ReadMatrixFromFile(secondMatrixPath);

        var resultParallel = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);
        var resultSequential = MatrixMultiplication.MultiplySequential(firstMatrix, secondMatrix);

        Assert.That(AreMatricesEqual(resultParallel, resultSequential), Is.True);
    }

    /// <summary>
    /// Validates that the <see cref="MatrixMultiplication.MultiplySequential"/> and
    /// <see cref="MatrixMultiplication.MultiplyParallel"/> methods throw an <see cref="ArgumentException"/>
    /// when provided with invalid matrix input files.
    /// </summary>
    /// <param name="firstMatrixPath">The file path to the first matrix input.</param>
    /// <param name="secondMatrixPath">The file path to the second matrix input.</param>
    [TestCaseSource(nameof(ArgumentExceptionTestCases))]
    public void TestArgumentException(string firstMatrixPath, string secondMatrixPath)
    {
        var firstMatrix = FileParser.ReadMatrixFromFile(firstMatrixPath);
        var secondMatrix = FileParser.ReadMatrixFromFile(secondMatrixPath);
        Assert.Throws<ArgumentException>(() => MatrixMultiplication.MultiplySequential(firstMatrix, secondMatrix));
    }

    /// <summary>
    /// Tests matrix multiplication using both parallel and sequential methods.
    /// It compares the results of both methods and verifies that they are identical for each test case.
    /// </summary>
    /// <param name="firstMatrix">The first matrix to multiply.</param>
    /// <param name="secondMatrix">The second matrix to multiply.</param>
    [TestCaseSource(nameof(HardcodedTestCases))]
    public void TestMultiplicationWithoutFile(int[,] firstMatrix, int[,] secondMatrix)
    {
        var resultParallel = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);
        var resultSequential = MatrixMultiplication.MultiplySequential(firstMatrix, secondMatrix);

        Assert.That(AreMatricesEqual(resultParallel, resultSequential), Is.True);
    }

    private static bool AreMatricesEqual(int[,] matrixA, int[,] matrixB)
    {
        if (matrixA.GetLength(0) != matrixB.GetLength(0) || matrixA.GetLength(1) != matrixB.GetLength(1))
        {
            return false;
        }

        for (var i = 0; i < matrixA.GetLength(0); ++i)
        {
            for (var j = 0; j < matrixA.GetLength(1); ++j)
            {
                if (matrixA[i, j] != matrixB[i, j])
                {
                    return false;
                }
            }
        }

        return true;
    }
}

