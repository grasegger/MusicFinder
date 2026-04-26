
public static class CliHelp
{
    public static void ShowHelp()
    {
        Console.WriteLine("MusicFinder CLI Help");
        Console.WriteLine("====================");
        Console.WriteLine();
        Console.WriteLine("Available Actions:");
        Console.WriteLine("  Import - Import albums from a CSV file");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- --MusicFinder:Action Import --MusicFinder:ImportPath path/to/file.csv");
        Console.WriteLine();
        Console.WriteLine("Available Actions:");
        Console.WriteLine("  Import - Import albums from a CSV file");
        Console.WriteLine("  Help   - Show this help (default)");
        Console.WriteLine();
        Console.WriteLine("Command line options:");
        Console.WriteLine("  --MusicFinder:Action <value>         - Action to perform (Import, Help)");
        Console.WriteLine("  --MusicFinder:ImportPath <path>      - Path to the CSV file to import");
        Console.WriteLine("  --MusicFinder:ImportDelimiter <char> - Delimiter used in CSV (default: ',')");
        Console.WriteLine("  --MusicFinder:DataDirectory <dir>    - Directory for data storage");
        Console.WriteLine("  --MusicFinder:DatabaseName <name>    - Database file name (default: musicfinder.db)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run -- --MusicFinder:Action Help");
        Console.WriteLine("  dotnet run -- --MusicFinder:Action Import --MusicFinder:ImportPath albums.csv");
    }
}