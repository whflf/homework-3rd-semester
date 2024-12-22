// <copyright file="MatrixMultiplication.cs" company="Elena Makarova">
// Copyright (c) Elena Makarova. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication;

/// <summary>
/// Provides methods for multiplying matrices, both sequentially and in parallel.
/// </summary>
public static class MatrixMultiplication
{
    /// <summary>
    /// Multiplies two matrices sequentially.
    /// </summary>
    /// <param name="firstMatrix">The first matrix to multiply.</param>
    /// <param name="secondMatrix">The second matrix to multiply.</param>
    /// <returns>A two-dimensional array representing the product of the two matrices.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of columns in the first matrix does not match the number of rows in the second matrix.</exception>
    public static int[,] MultiplySequential(int[,] firstMatrix, int[,] secondMatrix)
    {
        return Multiply(firstMatrix, secondMatrix, 0, firstMatrix.GetLength(0));
    }

    /// <summary>
    /// Multiplies two matrices in parallel.
    /// </summary>
    /// <param name="firstMatrix">The first matrix to multiply.</param>
    /// <param name="secondMatrix">The second matrix to multiply.</param>
    /// <returns>A two-dimensional array representing the product of the two matrices.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of columns in the first matrix does not match the number of rows in the second matrix.</exception>
    public static int[,] MultiplyParallel(int[,] firstMatrix, int[,] secondMatrix)
    {
        var firstMatrixRows = firstMatrix.GetLength(0);
        if (firstMatrixRows < Environment.ProcessorCount)
        {
            return MultiplySequential(firstMatrix, secondMatrix);
        }

        var secondMatrixColumns = secondMatrix.GetLength(1);
        var result = new int[firstMatrixRows, secondMatrixColumns];
        var threads = new Thread[Environment.ProcessorCount];
        var rowsPerThread = firstMatrixRows / threads.Length;
        var partialResults = new int[threads.Length][,];

        for (var i = 0; i < threads.Length; ++i)
        {
            var threadIndex = i;
            var startRow = threadIndex * rowsPerThread;
            var endRow = (threadIndex == threads.Length - 1) ? firstMatrixRows : startRow + rowsPerThread;

            threads[threadIndex] = new Thread(() =>
            {
                partialResults[threadIndex] = Multiply(firstMatrix, secondMatrix, startRow, endRow);

                for (var j = 0; j < partialResults[threadIndex].GetLength(0); ++j)
                {
                    for (var k = 0; k < secondMatrixColumns; ++k)
                    {
                        result[(threadIndex * rowsPerThread) + j, k] = partialResults[threadIndex][j, k];
                    }
                }
            });
            threads[threadIndex].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return result;
    }

    private static int[,] Multiply(int[,] firstMatrix, int[,] secondMatrix, int rowStart, int rowEnd)
    {
        var firstMatrixRows = firstMatrix.GetLength(0);
        var firstMatrixColumns = firstMatrix.GetLength(1);
        var secondMatrixRows = secondMatrix.GetLength(0);
        var secondMatrixColumns = secondMatrix.GetLength(1);

        if ((firstMatrixRows == 0 && firstMatrixColumns == 0) || (secondMatrixRows == 0 && secondMatrixColumns == 0))
        {
            throw new ArgumentException("Multiplier cannot be an empty matrix.");
        }

        var productMatrix = new int[rowEnd - rowStart, secondMatrixColumns];

        if (firstMatrixRows == 1 && firstMatrixColumns == 1)
        {
            return MultiplyMatrixByNumber(secondMatrix, firstMatrix[0, 0]);
        }
        else if (secondMatrixRows == 1 && secondMatrixColumns == 1)
        {
            return MultiplyMatrixByNumber(firstMatrix, secondMatrix[0, 0]);
        }

        if (firstMatrixColumns != secondMatrixRows)
        {
            throw new ArgumentException("Matrix sizes are different.");
        }

        var transposedSecondMatrix = new int[secondMatrixColumns, secondMatrixRows];
        for (var i = 0; i < secondMatrixRows; ++i)
        {
            for (var j = 0; j < secondMatrixColumns; ++j)
            {
                transposedSecondMatrix[j, i] = secondMatrix[i, j];
            }
        }

        for (var i = rowStart; i < rowEnd; ++i)
        {
            for (var j = 0; j < secondMatrixColumns; ++j)
            {
                for (var k = 0; k < firstMatrixColumns; ++k)
                {
                    productMatrix[i - rowStart, j] += firstMatrix[i, k] * transposedSecondMatrix[j, k];
                }
            }
        }

        return productMatrix;
    }

    private static int[,] MultiplyMatrixByNumber(int[,] matrix, int number)
    {
        var result = new int[matrix.GetLength(0), matrix.GetLength(1)];
        for (var i = 0; i < matrix.GetLength(0); ++i)
        {
            for (var j = 0; j < matrix.GetLength(1); ++j)
            {
                result[i, j] = matrix[i, j] * number;
            }
        }

        return result;
    }
}
