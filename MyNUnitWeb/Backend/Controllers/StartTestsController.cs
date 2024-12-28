using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

[ApiController]
[Route("api/startTests")]
public class StartTestsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var form = Request.Form;
        var files = form.Files;
        var isOk = true;

        foreach (var file in files)
        {
            var tempFolderName = Path.Combine("dedicated", $"temp{DateTime.Now.Ticks}");
            Directory.CreateDirectory(tempFolderName);

            var filePath = Path.Combine(tempFolderName, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                var command = $"dotnet dedicated/app/MyNUnitTestRun.dll {tempFolderName}";
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                process!.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var resultFileName = Path.Combine(tempFolderName, "result.txt");
            isOk = System.IO.File.Exists(resultFileName);
            if (isOk)
            {
                try
                {
                    var finalData = new FinalData();
                    var time = DateTime.Now.ToString("G");
                    var result = await System.IO.File.ReadAllTextAsync(resultFileName);
                    var builds = result.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var build in builds)
                    {
                        var tests = build.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                        if (tests.Length == 0)
                        {
                            continue;
                        }

                        finalData.buildName = tests[0];
                        finalData.time = time;
                        finalData.pass = new List<TestData>();
                        finalData.fail = new List<TestData>();
                        finalData.ignore = new List<TestData>();
                        finalData.custom = new List<TestData>();

                        foreach (var test in tests.Skip(1))
                        {
                            var testProperties = test.Split(" | ");
                            var testData = new TestData
                            {
                                status = testProperties.ElementAtOrDefault(0),
                                name = testProperties.ElementAtOrDefault(1),
                                time = testProperties.ElementAtOrDefault(2),
                                note = string.Join(" | ", testProperties.Skip(3))
                            };

                            switch (testData.status)
                            {
                                case "pass":
                                    finalData.pass.Add(testData);
                                    break;
                                case "fail":
                                    finalData.fail.Add(testData);
                                    break;
                                case "ignore":
                                    finalData.ignore.Add(testData);
                                    break;
                                default:
                                    finalData.custom.Add(testData);
                                    break;
                            }
                        }
                    }

                    var databaseFilePath = Path.Combine("dedicated", "history.json");
                    var updatedHistory = new List<FinalData>();

                    if (System.IO.File.Exists(databaseFilePath))
                    {
                        var historyContent = await System.IO.File.ReadAllTextAsync(databaseFilePath);
                        updatedHistory = JsonSerializer.Deserialize<List<FinalData>>(historyContent) ?? new List<FinalData>();
                    }

                    updatedHistory.Add(finalData);
                    await System.IO.File.WriteAllTextAsync(databaseFilePath, JsonSerializer.Serialize(updatedHistory));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Directory.Delete(tempFolderName, true);
        }

        return Ok(new { ok = isOk });
    }

    public class FinalData
    {
        public string buildName { get; set; }
        public string time { get; set; }
        public List<TestData> pass { get; set; }
        public List<TestData> fail { get; set; }
        public List<TestData> ignore { get; set; }
        public List<TestData> custom { get; set; }
    }

    public class TestData
    {
        [JsonIgnore]
        public string status { get; set; }
        public string name { get; set; }
        public string time { get; set; }
        public string note { get; set; }
    }
}
