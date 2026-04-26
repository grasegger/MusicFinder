using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FluentMigrator.Runner;
using MusicFinder.Models.Settings;
using MusicFinder.Models;
using MusicFinder.Actions;
using ActionEnum = MusicFinder.Models.Enums.Action;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<MusicFinderSettings>(hostContext.Configuration.GetSection("MusicFinder"));

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(GetConnectionString(hostContext))
                .ScanIn(typeof(Albums).Assembly).For.All());

        services.AddDbContext<MusicFinderContext>(
            options =>
            {
                options.UseSqlite(GetConnectionString(hostContext));
            }
        );

        services.AddScoped<Import>();
        services.AddScoped<BeetsDelete>();
    });

using var host = builder.Build();

var runner = host.Services.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

var config = host.Services.GetRequiredService<IOptions<MusicFinderSettings>>();

if (Enum.TryParse(config.Value.Action, out ActionEnum action))
{
    switch (action)
    {
        case ActionEnum.Import:
            var importAction = host.Services.GetRequiredService<Import>();
            await importAction.RunAsync(CancellationToken.None);
            break;
        case ActionEnum.BeetsDelete:
            var beetsDeleteAction = host.Services.GetRequiredService<BeetsDelete>();
            await beetsDeleteAction.RunAsync(CancellationToken.None);
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

static string GetConnectionString(HostBuilderContext hostContext)
{
    var settings = hostContext.Configuration.GetSection("MusicFinder").Get<MusicFinderSettings>() ?? new MusicFinderSettings();

    var dataDir = string.IsNullOrEmpty(settings.DataDirectory) ? Xdg.Directories.BaseDirectory.DataHome ?? "." : settings.DataDirectory;
    var musicDir = Path.Combine(dataDir, Assembly.GetExecutingAssembly().GetName().Name ?? "MusicFinder");
    Directory.CreateDirectory(musicDir);
    var dbPath = Path.Combine(musicDir, settings.DatabaseName);


    var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

    return connectionString;
}
