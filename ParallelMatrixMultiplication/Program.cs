using ParallelMatrixMultiplication;

try
{
    var firstMatrix = FileParsing.ReadMatrixFromFile(args[0]);
    var secondMatrix = FileParsing.ReadMatrixFromFile(args[1]);
    var result = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);

    FileParsing.WriteMatrixToFile("result1000x1500.txt", result);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
}
