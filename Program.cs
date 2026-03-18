using System.Formats.Tar;
using System.IO.Compression;

public class Program
{
    private static string _user = "Shorotshishir";
    private static string _repo = "gitignore";
    private static string _branch = "master";
    private static string targetUrl = "";

    private static async Task Main(string[] args)
    {
        string root = "https://github.com/";
        targetUrl = Path.Combine(root, _user, _repo, "archive/refs/heads", $"{_branch}.tar.gz");

        if (args.Length < 1)
        {
            ShowHelp();
            // await DoDownload();
            return;
        }

        if (args[0] == "help")
        {
            ShowHelp();
            return;
        }

        if (args[0] == "clean")
        {
            DoCleanup();
            return;
        }

        if (args[0] == "sync")
        {
            await DoSync();
            return;
        }



        var canonicalName = IgnoreDict.GetLanguage(args[0]);
        if (canonicalName != null)
        {
            Console.WriteLine($"Getting {canonicalName} gitignore...");
            await GetGitignoreFromArchive(canonicalName);
        }
        else
        {
            Console.WriteLine($"Language '{args[0]}' not found.");
            ShowHelp();
        }
    }

    private static async Task DoSync()
    {
        // make backup
        var gigDir = CreateGigDirIfNotExists();
        var existingFilePath = Path.Combine(gigDir, "gitignore.tar.gz");
        var backupFilePath = Path.Combine(gigDir, "gitignore.tar.gz.bak");

        if (File.Exists(existingFilePath))
        {
            File.Copy(existingFilePath, backupFilePath, true);
        }

        // try download
        await DoDownload(existingFilePath);
        // check success
        Console.WriteLine("gig is synced");
        // if fail use the backup
    }

    private static void DoCleanup()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        const string folder = ".gig";
        var fullPath = Path.Combine(home, folder);
        if (Directory.Exists(fullPath))
        {
            Console.WriteLine($"Found {fullPath}");
            var dirInfo = new DirectoryInfo(fullPath);
            foreach (var file in dirInfo.EnumerateFiles())
            {
                Console.WriteLine($"removing {file.Name}");
                file.Delete();
            }
        }
        else
        {
            Console.WriteLine($"Nothing to clean");
        }
    }

    private static void ShowHelp()
    {
        var keys = IgnoreDict.GetAllLanguages().ToList();
        int max = keys.Max(s => s.Length);

        Console.WriteLine("gig <command>");
        Console.WriteLine("");
        Console.WriteLine("Command");
        Console.WriteLine("--------");
        Console.WriteLine("help:\t show this message");
        Console.WriteLine("clean:\t delete all items from the .gig directory");
        Console.WriteLine("");
        Console.WriteLine("Example");
        Console.WriteLine("-------");
        Console.WriteLine("gig unity");
        Console.WriteLine("gig cpp");
        Console.WriteLine("gig c++");
        Console.WriteLine("");
        Console.WriteLine("All Commands available");
        Console.WriteLine("-------");

        foreach (var item in keys)
        {
            Console.WriteLine($"{item.PadRight(max)}:\tGenerate {item} gitignore");
        }
    }

    private static async Task GetGitignoreFromArchive(string targetLanguage)
    {
        var gigDir = CreateGigDirIfNotExists();
        var targetFile = Path.Combine(gigDir, "gitignore.tar.gz");

        var success = false;
        while (!success)
        {
            if (!File.Exists(targetFile))
            {
                await DoDownload(targetFile);
            }

            try
            {
                await using var fStream = new FileStream(targetFile, FileMode.Open, FileAccess.Read);
                await using var tSteam = new GZipStream(fStream, CompressionMode.Decompress);
                await using var reader = new TarReader(tSteam);

                while (await reader.GetNextEntryAsync() is { } entry)
                {
                    var path = entry.EntryType switch
                    {
                        TarEntryType.HardLink => $"{entry.Name} link to {entry.LinkName}",
                        TarEntryType.SymbolicLink => $"{entry.Name} -> {entry.LinkName}",
                        _ => entry.Name,
                    };

                    if (path.Contains(targetLanguage))
                    {
                        var file = Path.Combine(Environment.CurrentDirectory, ".gitignore");
                        await entry.ExtractToFileAsync(file, overwrite: true);
                        Console.WriteLine($"Created {file}");
                        break;
                    }
                }
                success = true;
            }
            catch (EndOfStreamException)
            {
                File.Delete(targetFile);
            }
        }
    }

    private static async Task DoDownload(string targetFile)
    {
        var client = new HttpClient();
        await using var fStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
        await using var dStream = await client.GetStreamAsync(targetUrl);
        await dStream.CopyToAsync(fStream);
        await fStream.FlushAsync();
        Console.WriteLine("Downloaded root");
    }

    private static string CreateGigDirIfNotExists()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        const string folder = ".gig";
        var fullPath = Path.Combine(home, folder);
        if (Directory.Exists(fullPath))
        {
            Console.WriteLine($"Found {fullPath}");
            return fullPath;
        }
        try
        {
            var di = Directory.CreateDirectory(fullPath);
            Console.WriteLine($"Directory created successfully at: {di.FullName}");
        }
        catch (IOException e)
        {
            Console.WriteLine($"IO Error: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }
        return fullPath;
    }
}
