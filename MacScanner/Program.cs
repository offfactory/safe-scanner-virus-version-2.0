using System.Security.Cryptography;

Console.WriteLine("SafeScan Defender macOS scanner");
Console.WriteLine("Local-only SHA-256 scan. This command does not upload file contents.");

var target = args.Length > 0 ? args[0] : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
if (!Directory.Exists(target))
{
    Console.Error.WriteLine($"Directory not found: {target}");
    return 2;
}

var scanned = 0;
var skipped = 0;
foreach (var file in EnumerateFilesSafely(target))
{
    try
    {
        await using var stream = File.OpenRead(file);
        var hash = await SHA256.HashDataAsync(stream);
        Console.WriteLine($"{Convert.ToHexString(hash).ToLowerInvariant()}  {file}");
        scanned++;
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
    {
        skipped++;
    }
}

Console.WriteLine($"Completed. Files scanned: {scanned}; files skipped: {skipped}.");
return 0;

static IEnumerable<string> EnumerateFilesSafely(string root)
{
    var pending = new Stack<string>();
    pending.Push(root);

    while (pending.Count > 0)
    {
        var directory = pending.Pop();
        string[] files;
        try
        {
            files = Directory.GetFiles(directory);
        }
        catch (IOException)
        {
            continue;
        }
        catch (UnauthorizedAccessException)
        {
            continue;
        }

        foreach (var file in files)
        {
            yield return file;
        }

        string[] children;
        try
        {
            children = Directory.GetDirectories(directory);
        }
        catch (IOException)
        {
            continue;
        }
        catch (UnauthorizedAccessException)
        {
            continue;
        }

        foreach (var child in children)
        {
            pending.Push(child);
        }
    }
}
