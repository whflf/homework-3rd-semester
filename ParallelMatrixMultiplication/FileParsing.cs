// <copyright file="FileParsing.cs" company="Elena Makarova">
// Copyright (c) Elena Makarova. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication;

/// <summary>
/// Provides methods for reading and writing matrices to and from files.
/// </summary>
public static class FileParsing
{
    /// <summary>
    /// Reads a matrix from a file and returns it as a two-dimensional array.
    /// </summary>
    /// <param name="filePath">The path to the file containing the matrix data.</param>
    /// <returns>A two-dimensional array representing the matrix.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="FormatException">Thrown when the file content is not in the expected format.</exception>
    public static int[,] ReadMatrixFromFile(string filePath)
    {
        var linesArray = File.ReadAllLines(filePath);
        var matrix = new int[linesArray.Length, (linesArray[0].Length + 1) / 2];

        for (var i = 0; i < linesArray.Length; ++i)
        {
            var numbersArray = linesArray[i].Split(' ');
            for (var j = 0; j < numbersArray.Length; ++j)
            {
                matrix[i, j] = int.Parse(numbersArray[j]);
            }
        }

        return matrix;
    }

    /// <summary>
    /// Writes a matrix to a file.
    /// </summary>
    /// <param name="filePath">The path to the file where the matrix will be written.</param>
    /// <param name="matrix">The two-dimensional array representing the matrix to be written.</param>
    /// <exception cref="IOException">Thrown when there is an issue writing to the file.</exception>
    public static void WriteMatrixToFile(string filePath, int[,] matrix)
    {
        using var writer = new StreamWriter(filePath);

        for (var i = 0; i < matrix.GetLength(0); ++i)
        {
            for (var j = 0; j < matrix.GetLength(1); ++j)
            {
                writer.Write(matrix[i, j].ToString() + ' ');
            }

            writer.WriteLine();
        }
    }
}
