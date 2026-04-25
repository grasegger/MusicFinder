using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using FluentMigrator.Runner;
using System.Reflection;
using MusicFinder.Models.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using ActionEnum = MusicFinder.Models.Enums.Action;
using MusicFinder.Actions;
using MusicFinder.Actions.CLI;
using MusicFinder.Models;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var settings = hostContext.Configuration.GetSection("MusicFinder").Get<MusicFinderSettings>() ?? new MusicFinderSettings();

        var dataDir = string.IsNullOrEmpty(settings.DataDirectory) ? Xdg.Directories.BaseDirectory.DataHome ?? "." : settings.DataDirectory;
        var musicDir = Path.Combine(dataDir, Assembly.GetExecutingAssembly().GetName().Name ?? "MusicFinder");
        Directory.CreateDirectory(musicDir);
        var dbPath = Path.Combine(musicDir, settings.DatabaseName);
        File.Create(dbPath).Close();
        var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

        services.AddSingleton(Options.Create(settings));
        services.AddSingleton(settings);
        services.AddSingleton(sp => connectionString);
        services.AddScoped(sp => new MusicFinderContext());
        services.AddScoped<Import>();
    });

using var host = builder.Build();

using var scope = host.Services.CreateScope();

var config = scope.ServiceProvider.GetRequiredService<IOptions<MusicFinderSettings>>();
var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
migrator.MigrateUp();

var importAction = scope.ServiceProvider.GetRequiredService<Import>();

Console.WriteLine($"Action: {config.Value.Action}, Path: {config.Value.ImportPath}");

if (Enum.TryParse(config.Value.Action, out ActionEnum action))
{
    switch (action)
    {
        case ActionEnum.Import:
            await importAction.RunAsync(CancellationToken.None);
            break;
        case ActionEnum.Help:
        default:
            CliHelp.ShowHelp();
            break;
    }
}
else
{
    Console.WriteLine($"Invalid action: {config.Value.Action}");
    CliHelp.ShowHelp();
}


await host.StopAsync();