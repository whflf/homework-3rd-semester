#pragma warning disable CS4014

using System.Net;
using System.Net.Sockets;
using System.Text;

const int port = 8888;
var listener = new TcpListener(IPAddress.Any, port);
listener.Start();

while (true)
{
    var socket = await listener.AcceptSocketAsync();
    Console.WriteLine("New client has been connected");
    Task.Run(async () => await HandleClient(socket));
}

async Task ListFilesAndDirectories(string path, NetworkStream stream)
{
    try
    {
        var entries = Directory.GetFileSystemEntries(path, "*");
        var response = new List<byte>();
        response.AddRange(BitConverter.GetBytes(entries.Length));

        foreach (var entry in entries)
        {
            response.AddRange(Encoding.UTF8.GetBytes(entry));
            response.Add(Directory.Exists(entry) ? (byte)1 : (byte)0);
        }

        await stream.WriteAsync(BitConverter.GetBytes(response.Count));
        await stream.WriteAsync(response.ToArray());
    }
    catch (DirectoryNotFoundException)
    {
        await stream.WriteAsync(BitConverter.GetBytes(-1));
    }
    finally
    {
        await stream.FlushAsync();
    }
}

async Task SendFileToClient(string filePath, NetworkStream stream)
{
    try
    {
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        await stream.WriteAsync(BitConverter.GetBytes(fileBytes.Length));
        await stream.WriteAsync(fileBytes);
    }
    catch (FileNotFoundException)
    {
        await stream.WriteAsync(BitConverter.GetBytes(-1));
    }
    finally
    {
        await stream.FlushAsync();
    }
}

async Task HandleClient(Socket socket)
{
    var stream = new NetworkStream(socket);
    var reader = new StreamReader(stream);

    while (true)
    {
        var query = await reader.ReadLineAsync();

        if (query is null)
        {
            return;
        }

        var splittedQuery = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var writer = new StreamWriter(stream);
        switch (splittedQuery[0])
        {
            case "1":
                await ListFilesAndDirectories(splittedQuery[1], stream);
                break;
            case "2":
                Console.WriteLine("Sending file {0} to the client...", splittedQuery[1]);
                await SendFileToClient(splittedQuery[1], stream);
                break;
            case "3":
                socket.Close();
                return;
            default:
                await stream.WriteAsync(BitConverter.GetBytes(-1));
                break;
        }
    }
}
