using System.Diagnostics;

var path = "../../../";

var stopwatch = new Stopwatch();
stopwatch.Start();
var checkSumSync = await CheckSums.GetCheckSumSingleThread(path);
stopwatch.Stop();
var deltaSync = stopwatch.ElapsedMilliseconds;

stopwatch.Restart();
var checkSumAsync = await CheckSums.GetCheckSumSingleThread(path);
stopwatch.Stop();
var deltaAsync = stopwatch.ElapsedMilliseconds;

Console.WriteLine($"Check sum sync: {BitConverter.ToString(checkSumSync)}\n Time: {deltaSync} ms\nCheck sum async: {BitConverter.ToString(checkSumAsync)}\n Time: {deltaAsync} ms");
