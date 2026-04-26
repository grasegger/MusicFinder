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

var actionTypes = typeof(CliHelp).Assembly.GetTypes()
            .Where(t => t.Namespace == "MusicFinder.Actions" && t.IsClass && !t.IsAbstract && t.IsVisible)
            .ToList();

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

        foreach (var actionType in actionTypes)
        {
            services.AddScoped(actionType);
        }
    });

using var host = builder.Build();

var runner = host.Services.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

var config = host.Services.GetRequiredService<IOptions<MusicFinderSettings>>();


var targetAction = actionTypes.FirstOrDefault(t => t.Name == config.Value.Action) ?? typeof(CliHelp);

Console.WriteLine($"Running action '{config.Value.Action}'");
host.Services.GetRequiredService(targetAction);
await ((dynamic)host.Services.GetRequiredService(targetAction)).RunAsync(CancellationToken.None);


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
