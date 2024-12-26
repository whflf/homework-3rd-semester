using ParallelMatrixMultiplication;

try
{
    var firstMatrix = FileParser.ReadMatrixFromFile(args[0]);
    var secondMatrix = FileParser.ReadMatrixFromFile(args[1]);
    var result = MatrixMultiplication.MultiplyParallel(firstMatrix, secondMatrix);

    FileParser.WriteMatrixToFile("result1000x1500.txt", result);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
}
