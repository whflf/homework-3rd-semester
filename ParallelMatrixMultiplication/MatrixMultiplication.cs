namespace ParallelMatrixMultiplication;

public static class MatrixMultiplication
{
    public static int[,] MultiplySequential(int[,] firstMatrix, int[,] secondMatrix)
    {
        var firstMatrixRows = firstMatrix.GetLength(0);
        var product = new int[firstMatrixRows, secondMatrix.GetLength(1)];
        Multiply((firstMatrix, secondMatrix, product, 0, firstMatrixRows));

        return product;
    }

    public static int[,] MultiplyParallel(int[,] firstMatrix, int[,] secondMatrix)
    {
        var firstMatrixRows = firstMatrix.GetLength(0);
        var secondMatrixColumns = secondMatrix.GetLength(1);
        var product = new int[firstMatrixRows, secondMatrixColumns];
        var threads = new Thread[firstMatrixRows];

        for (var i = 0; i < threads.Length; ++i)
        {
            threads[i] = new Thread(new ParameterizedThreadStart(Multiply));
        }

        for (var i = 0; i < threads.Length; ++i)
        {
            var localI = i;
            threads[i].Start((firstMatrix, secondMatrix, product, localI, localI + 1));
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return product;
    }

    private static void Multiply(object? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var (firstMatrix, secondMatrix, productMatrix, rowStart, rowEnd) = 
            ((int[,], int[,], int[,], int, int))data;

        var firstMatrixColumns = firstMatrix.GetLength(1);
        var secondMatrixRows = secondMatrix.GetLength(0);
        var secondMatrixColumns = secondMatrix.GetLength(1);

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
                    productMatrix[i, j] += firstMatrix[i, k] * transposedSecondMatrix[j, k];
                }
            }
        }
    }
}
