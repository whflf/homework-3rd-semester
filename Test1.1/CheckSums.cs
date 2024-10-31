using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Provides methods for calculating checksums of files and directories using MD5 hashing.
/// </summary>
public static class CheckSums
{
    /// <summary>
    /// Computes the MD5 checksum of a file or directory in a single-threaded manner.
    /// If the specified path is a file, it returns the hash of the file. 
    /// If the path is a directory, it computes the checksum recursively 
    /// by including the names of all files and subdirectories.
    /// </summary>
    /// <param name="path">The path to the file or directory for which to compute the checksum.</param>
    /// <returns>A <see cref="Task{Byte[]}"/> representing the asynchronous operation, containing the computed MD5 checksum as a byte array.</returns>
    public static async Task<byte[]> GetCheckSumSingleThread(string path)
    {
        if (!Directory.Exists(path))
        {
            var hash = await GetFileHash(path);
            return hash;
        }

        var directoryElements = Directory.GetFileSystemEntries(path);
        Array.Sort(directoryElements);

        var checkSum = new List<byte>();
        var directoryName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
        checkSum.AddRange(Encoding.UTF8.GetBytes(directoryName));

        foreach (var element in directoryElements)
        {
            checkSum.AddRange(await GetCheckSumSingleThread(path + element));
        }

        using (var md5 = MD5.Create())
            return md5.ComputeHash(checkSum.ToArray());
    }

    /// <summary>
    /// Computes the MD5 checksum of a file or directory asynchronously.
    /// If the specified path is a file, it returns the hash of the file. 
    /// If the path is a directory, it computes the checksum recursively 
    /// by including the names of all files and subdirectories.
    /// This method executes tasks in parallel for subdirectories.
    /// </summary>
    /// <param name="path">The path to the file or directory for which to compute the checksum.</param>
    /// <returns>A <see cref="Task{Byte[]}"/> representing the asynchronous operation, containing the computed MD5 checksum as a byte array.</returns>
    public static async Task<byte[]> GetCheckSumAsync(string path)
    {
        if (!Directory.Exists(path))
        {
            var hash = await GetFileHash(path);
            return hash;
        }

        var directoryElements = Directory.GetFileSystemEntries(path);
        Array.Sort(directoryElements);

        var checkSum = new List<byte>();
        var directoryName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));
        checkSum.AddRange(Encoding.UTF8.GetBytes(directoryName));

        var tasks = new List<Task<byte[]>>();
        foreach (var element in directoryElements)
        {
            tasks.Add(Task.Run(() => { return GetCheckSumAsync(path + element); } ));
        }
        await Task.WhenAll(tasks);
        foreach (var task in tasks)
        {
            checkSum.AddRange(task.Result);
        }

        using (var md5 = MD5.Create())
            return md5.ComputeHash(checkSum.ToArray());
    }

    private static async Task<byte[]> GetFileHash(string filePath)
    {
        try
        {
            var bufferSize = 4096;
            using (var md5 = MD5.Create())
            using (var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: bufferSize,
                useAsync: true))
            {
                var buffer = new byte[bufferSize];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                }

                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                return md5.Hash;
            }
        }
        catch (FileNotFoundException)
        {
            return Array.Empty<byte>();
        }
    }
}

