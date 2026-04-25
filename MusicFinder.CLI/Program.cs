using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using MusicFinder.Models.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using ActionEnum = MusicFinder.Models.Enums.Action;
using MusicFinder.Actions;
using MusicFinder.Actions.CLI;
using MusicFinder.Models;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<MusicFinderSettings>(hostContext.Configuration.GetSection("MusicFinder"));

        services.AddDbContext<MusicFinderContext>(
            options =>
            {
                var settings = hostContext.Configuration.GetSection("MusicFinder").Get<MusicFinderSettings>() ?? new MusicFinderSettings();

                var dataDir = string.IsNullOrEmpty(settings.DataDirectory) ? Xdg.Directories.BaseDirectory.DataHome ?? "." : settings.DataDirectory;
                var musicDir = Path.Combine(dataDir, Assembly.GetExecutingAssembly().GetName().Name ?? "MusicFinder");
                Directory.CreateDirectory(musicDir);
                var dbPath = Path.Combine(musicDir, settings.DatabaseName);
                if (!File.Exists(dbPath))
                {
                    File.Create(dbPath).Close();
                }
                var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
                options.UseSqlite(connectionString);
            }
        );

        services.AddScoped<Import>();
    });

using var host = builder.Build();

var db = host.Services.GetRequiredService<MusicFinderContext>();
await db.Database.MigrateAsync();

var config = host.Services.GetRequiredService<IOptions<MusicFinderSettings>>();

if (Enum.TryParse(config.Value.Action, out ActionEnum action))
{
    switch (action)
    {
        case ActionEnum.Import:
            var importAction = host.Services.GetRequiredService<Import>();
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