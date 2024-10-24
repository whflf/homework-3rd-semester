using System.Net.Sockets;
using System.Text;

const int port = 8888;
using (var client = new TcpClient("localhost", port))
{
    Console.WriteLine("""
        Commands and queries:
            1 <path: String>\n - list files of the certain directory on the server
            2 <path: String>\n - download file with a given path from the server
            3 - disconnect from the server and exit
        """);
    var stream = client.GetStream();
    await CommunicateWithServer(stream);
}

async Task CommunicateWithServer(NetworkStream stream)
{
    var writer = new StreamWriter(stream);
    var reader = new StreamReader(stream);
    var isConnected = true;

    while (isConnected)
    {
        var query = Console.ReadLine();
        if (query is null || query[0] < '1' || query[0] > '3')
        {
            Console.WriteLine("Bad input. Please use one of the query codes listed above.");
            continue;
        }

        await writer.WriteLineAsync(query);
        await writer.FlushAsync();

        byte[] buffer = await ReadBytesFromStream(stream);
        switch (query[0])
        {
            case '1':
                var result = await DeserializeDirectoryList(buffer);
                if (result != "-1 ")
                {
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("No such directory has been found.");
                }
                break;
            case '2':
                if (buffer is not null)
                {
                    var fileName = query.Split('/')[^1];
                    await File.WriteAllBytesAsync(fileName, buffer);
                    Console.WriteLine($"File received and saved as {fileName}");
                }
                break;
            case '3':
                Console.WriteLine("You've been disconnected from the server. See you again!");
                isConnected = false;
                break;
        }
    }
}

async Task<string> DeserializeDirectoryList(byte[] buffer)
{
    var result = BitConverter.ToInt32(buffer).ToString() + ' ';
    var startIndex = 4;

    await Task.Run(() =>
    {
        for (var i = 4; i < buffer.Length; ++i)
        {
            if (buffer[i] != 0 && buffer[i] != 1)
            {
                continue;
            }

            result += Encoding.UTF8.GetString(buffer, startIndex, i - startIndex);
            startIndex = i + 1;

            if (buffer[i] == 0)
            {
                result += " false ";
            }
            else
            {
                result += " true ";
            }
        }
    });

    return result;
}

async Task<byte[]> ReadBytesFromStream(NetworkStream stream)
{
    var sizeBuffer = new byte[sizeof(int)];

    await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);
    var size = BitConverter.ToInt32(sizeBuffer);
    if (size == -1)
    {
        Console.WriteLine("No such file has been found.");
        return new byte[] { 0 };
    }

    var buffer = new byte[size];
    await stream.ReadAsync(buffer, 0, size);

    return buffer;
}
