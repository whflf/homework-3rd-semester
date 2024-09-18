namespace ParallelMatrixMultiplication;

public static class FileParsing
{
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
