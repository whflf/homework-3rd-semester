// <copyright file="SimpleFTPTests.cs" company="Elena Makarova">
// Copyright (c) Elena Makarova. All rights reserved.
// </copyright>

#pragma warning disable SA1615
namespace SimpleFTP.Tests;

using System.Net.Sockets;
using System.Text;

/// <summary>
/// Contains tests for the SimpleFTP server.
/// </summary>
[TestFixture]
public class SimpleFTPTests
{
    private const int Port = 8888;
    private const string ServerAddress = "127.0.0.1";
    private const string TestPath = "../../../../SimpleFTP.Tests/DirectoryForTests";

    /// <summary>
    /// Tests if the client can successfully connect to the server.
    /// </summary>
    [Test]
    public async Task TestClientConnection()
    {
        using (var client = new TcpClient())
        {
            await client.ConnectAsync(ServerAddress, Port);
            Assert.That(client.Connected, Is.True);
        }
    }

    /// <summary>
    /// Tests if the server correctly lists files and directories for a valid path.
    /// </summary>
    [Test]
    public async Task TestListFilesAndDirectories_ValidPath()
    {
        var validResponse = new List<byte>();
        var entries = Directory.GetFileSystemEntries(TestPath, "*");
        validResponse.AddRange(BitConverter.GetBytes(entries.Length));
        foreach (var entry in entries)
        {
            validResponse.AddRange(Encoding.UTF8.GetBytes(entry));
            validResponse.Add(Directory.Exists(entry) ? (byte)1 : (byte)0);
        }

        using (var client = new TcpClient(ServerAddress, Port))
        {
            var stream = client.GetStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };

            await writer.WriteLineAsync($"1 {TestPath}");

            var responseLength = this.GetResponseLength(stream);

            Assert.That(responseLength > 0);

            var buffer = new byte[responseLength];
            await stream.ReadAsync(buffer, 0, buffer.Length);

            for (var i = 0; i < responseLength; ++i)
            {
                Assert.That(buffer[i], Is.EqualTo(validResponse[i]));
            }
        }
    }

    /// <summary>
    /// Tests if the server correctly handles an invalid path.
    /// </summary>
    [Test]
    public async Task TestListFilesAndDirectories_InvalidPath()
    {
        using (var client = new TcpClient(ServerAddress, Port))
        {
            var stream = client.GetStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };

            var invalidPath = "invalid_path";
            await writer.WriteLineAsync($"1 {invalidPath}");

            var responseLength = this.GetResponseLength(stream);

            Assert.That(responseLength == -1);
        }
    }

    /// <summary>
    /// Tests if the server correctly sends a file to the client for a valid file path.
    /// </summary>
    /// <param name="path">The path to the file to be sent.</param>
    [TestCase($"{TestPath}/picture.jpg")]
    [TestCase($"{TestPath}/text-file.txt")]
    public async Task TestSendFileToClient_ValidFile(string path)
    {
        var fileBytes = await File.ReadAllBytesAsync(path);
        using (var client = new TcpClient(ServerAddress, Port))
        {
            var stream = client.GetStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };

            await writer.WriteLineAsync($"2 {path}");

            var responseLength = this.GetResponseLength(stream);

            Assert.That(responseLength > 0);

            var buffer = new byte[responseLength];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            for (var i = 0; i < responseLength; ++i)
            {
                Assert.That(buffer[i], Is.EqualTo(fileBytes[i]));
            }
        }
    }

    /// <summary>
    /// Tests if the server correctly handles an invalid file path.
    /// </summary>
    [Test]
    public async Task TestSendFileToClient_InvalidFile()
    {
        using (var client = new TcpClient(ServerAddress, Port))
        {
            var stream = client.GetStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };

            var invalidFile = "invalid_file.txt";
            await writer.WriteLineAsync($"2 {invalidFile}");

            var responseLength = this.GetResponseLength(stream);

            Assert.That(responseLength == -1);
        }
    }

    /// <summary>
    /// Tests if the server correctly handles an invalid query.
    /// </summary>
    [Test]
    public async Task TestInvalidQuery()
    {
        using (var client = new TcpClient(ServerAddress, Port))
        {
            var stream = client.GetStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };

            await writer.WriteLineAsync("invalid_query");

            var responseLength = this.GetResponseLength(stream);

            Assert.That(responseLength == -1);
        }
    }

    /// <summary>
    /// Tests if the server can handle multiple parallel requests.
    /// </summary>
    [Test]
    public async Task TestParallelRequests()
    {
        var numberOfClients = 10;
        var tasks = new Task[numberOfClients];
        var startTimes = new DateTime[numberOfClients];
        var endTimes = new DateTime[numberOfClients];

        for (var i = 0; i < numberOfClients; ++i)
        {
            var clientIndex = i;
            tasks[i] = new Task(async () =>
            {
                using (var client = new TcpClient(ServerAddress, Port))
                {
                    startTimes[clientIndex] = DateTime.Now;
                    var stream = client.GetStream();
                    var writer = new StreamWriter(stream) { AutoFlush = true };
                    var reader = new StreamReader(stream);

                    await writer.WriteLineAsync($"1 {TestPath}");

                    var responseLength = this.GetResponseLength(stream);
                    var buffer = new byte[responseLength];
                    await stream.ReadAsync(buffer, 0, buffer.Length);

                    endTimes[clientIndex] = DateTime.Now;
                }
            });
        }

        foreach (var task in tasks)
        {
            task.Start();
        }

        await Task.WhenAll(tasks);

        var earliestStart = startTimes.Min();
        var latestEnd = endTimes.Max();

        var totalDuration = latestEnd - earliestStart;
        var maxIndividualDuration = TimeSpan.Zero;

        for (var i = 0; i < numberOfClients; ++i)
        {
            var individualDuration = endTimes[i] - startTimes[i];
            if (individualDuration > maxIndividualDuration)
            {
                maxIndividualDuration = individualDuration;
            }
        }

        Assert.Less(totalDuration, maxIndividualDuration * numberOfClients);
    }

    private int GetResponseLength(NetworkStream stream)
    {
        var responseLengthBytes = new byte[sizeof(int)];
        stream.Read(responseLengthBytes, 0, responseLengthBytes.Length);
        return BitConverter.ToInt32(responseLengthBytes, 0);
    }
}
