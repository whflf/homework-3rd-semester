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
    /// <summary>
    /// Tests multiplication of two 500x500 matrices using both parallel and sequential methods,
    /// and verifies that the results are identical.
    /// </summary>
    [Test]
    public void Test500X500MultiplyBy500X500()
    {
        var firstMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test500x500.txt");
        var secondMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test500x500.txt");
        var resultParallel = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);
        var resultSequential = MatrixMultiplication.MultiplySequential(firstMatrix, secondMatrix);
        var match = true;

        for (var i = 0; i < resultParallel.GetLength(0); ++i)
        {
            for (var j = 0; j < resultParallel.GetLength(1); ++j)
            {
                if (resultParallel[i, j] != resultSequential[i, j])
                {
                    match = false;
                    break;
                }
            }
        }

        Assert.That(match, Is.True);
    }

    /// <summary>
    /// Tests multiplication of a 1000x500 matrix by a 500x500 matrix using both parallel and sequential methods,
    /// and verifies that the results are identical.
    /// </summary>
    [Test]
    public void Test1000X500MultiplyBy500X500()
    {
        var firstMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test1000x500.txt");
        var secondMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test500x500.txt");
        var resultParallel = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);
        var resultSequential = MatrixMultiplication.MultiplySequential(firstMatrix, secondMatrix);
        var match = true;

        for (var i = 0; i < resultParallel.GetLength(0); ++i)
        {
            for (var j = 0; j < resultParallel.GetLength(1); ++j)
            {
                if (resultParallel[i, j] != resultSequential[i, j])
                {
                    match = false;
                    break;
                }
            }
        }

        Assert.That(match, Is.True);
    }

    /// <summary>
    /// Tests multiplication of a 1000x1500 matrix by a 1500x1500 matrix using both parallel and sequential methods,
    /// and verifies that the results are identical.
    /// </summary>
    [Test]
    public void Test1000X1500MultiplyBy1500X1500()
    {
        var firstMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test1000x1500.txt");
        var secondMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test1500x1500.txt");
        var resultParallel = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);
        var resultSequential = MatrixMultiplication.MultiplySequential(firstMatrix, secondMatrix);
        var match = true;

        for (var i = 0; i < resultParallel.GetLength(0); ++i)
        {
            for (var j = 0; j < resultParallel.GetLength(1); ++j)
            {
                if (resultParallel[i, j] != resultSequential[i, j])
                {
                    match = false;
                    break;
                }
            }
        }

        Assert.That(match, Is.True);
    }

    /// <summary>
    /// Tests multiplication of a 2000x2500 matrix by a 2500x1500 matrix using both parallel and sequential methods,
    /// and verifies that the results are identical.
    /// </summary>
    [Test]
    public void Test2000X2500MultiplyBy2500X1500()
    {
        var firstMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test2000x2500.txt");
        var secondMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test2500x1500.txt");
        var resultParallel = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);
        var resultSequential = MatrixMultiplication.MultiplySequential(firstMatrix, secondMatrix);
        var match = true;

        for (var i = 0; i < resultParallel.GetLength(0); ++i)
        {
            for (var j = 0; j < resultParallel.GetLength(1); ++j)
            {
                if (resultParallel[i, j] != resultSequential[i, j])
                {
                    match = false;
                    break;
                }
            }
        }

        Assert.That(match, Is.True);
    }

    /// <summary>
    /// Tests multiplication of a 3000x3000 matrix by a 3000x1500 matrix using both parallel and sequential methods,
    /// and verifies that the results are identical.
    /// </summary>
    [Test]
    public void Test3000X3000MultiplyBy3000X1500()
    {
        var firstMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test3000x3000.txt");
        var secondMatrix = FileParsing.ReadMatrixFromFile("./TestFiles/test3000x1500.txt");
        var resultParallel = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);
        var resultSequential = MatrixMultiplication.MultiplySequential(firstMatrix, secondMatrix);
        var match = true;

        for (var i = 0; i < resultParallel.GetLength(0); ++i)
        {
            for (var j = 0; j < resultParallel.GetLength(1); ++j)
            {
                if (resultParallel[i, j] != resultSequential[i, j])
                {
                    match = false;
                    break;
                }
            }
        }

        Assert.That(match, Is.True);
    }
}
