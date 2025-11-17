using System.Text;
using System.Threading.Tasks;

public class Program
{
    private static readonly string root = "https://raw.githubusercontent.com/github/gitignore/refs/heads/main/";
    private static readonly StringBuilder sb = new();
    private static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            ShowHelp();
            return;
        }

        if (args[0] == "help")
        {
            ShowHelp();
            return;
        }

        if (IgnoreDict.AvailableLangs.TryGetValue(args[0], out var additionalPath))
        {
            var fullPath = $"{root}{additionalPath}.gitignore";
            Console.WriteLine(fullPath);
            await WriteContent(await GetContent(fullPath));
        }
    }

    private static async Task WriteContent(Stream stream)
    {
        using var fstream = File.Create(Path.Combine(Environment.CurrentDirectory, ".gitignore"));
        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(fstream);
        System.Console.WriteLine("Created ...");
    }

    private static async Task<Stream> GetContent(string url)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1;
        var input = await response.Content.ReadAsStreamAsync();
        var output = new MemoryStream();

        var buffer = new byte[81920];
        long read = 0;
        int bytes;

        while ((bytes = await input.ReadAsync(buffer)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, bytes));
            read += bytes;

            if (total > 0)
                Console.Write($"\rProgress: {read * 100.0 / total:0.0}%");
            else
                Console.Write($"\rDownloaded {read / 1024} KB");
        }

        Console.WriteLine();
        output.Position = 0;
        return output;
    }

    private static void ShowHelp()
    {
        var keys = IgnoreDict.AvailableLangs.Keys
            .Select(k => k.ToLowerInvariant())
            .Distinct()
            .ToList();
        int max = keys.Max(s => s.Length);

        sb.AppendLine("gig <command> ");
        sb.AppendLine("");
        sb.AppendLine("Command\n--------");
        sb.AppendLine($"help:\t show this message");
        sb.AppendLine("");
        sb.AppendLine("Example\n-------");
        sb.AppendLine("gig unity");
        sb.AppendLine("gig Unity");
        sb.AppendLine("");
        sb.AppendLine("All Commands available\n-------");

        foreach (var item in keys)
        {
            sb.AppendLine($"{item.PadRight(max)}:\tGenerate {item} gitignore");
        }
        Console.WriteLine(sb.ToString());
        sb.Clear();
    }
}