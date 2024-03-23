using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ArcadePointsBot.Auth;
using ArcadePointsBot.Config;
using ArcadePointsBot.Data.Abstractions.Repositories;
using ArcadePointsBot.Data.Contexts;
using ArcadePointsBot.Data.Repositories;
using ArcadePointsBot.Domain.Rewards;
using ArcadePointsBot.Infrastructure.Interop;
using ArcadePointsBot.Services;
using ArcadePointsBot.ViewModels;
using ArcadePointsBot.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ArcadePointsBot;

public partial class App : Avalonia.Application
{
    private static ILogger<App> _appLogger = null!;
    private static IMediator _appMessenger = null!;
    public IHost? GlobalHost { get; private set; }

    private void EnsureDb(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        _appLogger.LogInformation("Checking for Db updates");
        try
        {
            using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrations = db.Database.GetPendingMigrations();
            if (migrations.Any())
            {
                _appLogger.LogInformation("Found {count} updates, updating DB", migrations.Count());
                db.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            _appLogger.LogError(ex, "An Error occurred updating the DB");
            throw;
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        GlobalHost = CreateAppBuilder().Build();
        var appScope = GlobalHost.Services.CreateScope();
        var loggerFactory = appScope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _appLogger = loggerFactory.CreateLogger<App>();
        _appMessenger = appScope.ServiceProvider.GetRequiredService<IMediator>();
        EnsureDb(GlobalHost.Services);

        var lang = GlobalHost
            .Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("lang");
        ArcadePointsBot.Resources.L10n.Culture = new System.Globalization.CultureInfo(
            lang ?? "en-US"
        );
        ArcadePointsBot.Resources.Enums.Culture = new System.Globalization.CultureInfo(
            lang ?? "en-US"
        );

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            desktop.MainWindow = new MainWindow
            {
                DataContext = GlobalHost.Services.GetRequiredService<MainWindowViewModel>(),
            };

            desktop.ShutdownRequested += ShutDown;
            desktop.Exit += (_, _) =>
            {
                GlobalHost.Dispose();
                appScope.Dispose();
                GlobalHost = null;
            };
        }

        DataTemplates.Add(GlobalHost.Services.GetRequiredService<ViewLocator>());

        base.OnFrameworkInitializationCompleted();

        await GlobalHost.StartAsync();
    }

    private void ShutDown(object? sender, ShutdownRequestedEventArgs e)
    {
        _appLogger.LogTrace("App shutdown requested by, {sender}", sender);
        _appLogger.LogInformation("Stopping App");

        _appLogger.LogTrace("Disabling rewards");

        var task = Task.Factory.StartNew(
            async () =>
            {
                var res = await _appMessenger.Send(new Application.Rewards.DisableRewardsCommand());
                _appLogger.LogTrace("Rewards Disabled: {success}", res.IsSuccess);
            },
            TaskCreationOptions.LongRunning
        );
        Task.Delay(2000).Wait();
        GlobalHost!.StopAsync();
    }

    public static IHostBuilder CreateAppBuilder() =>
        Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
            .UseSerilog(
                (ctx, svc, cfg) =>
                {
                    cfg.ReadFrom.Configuration(ctx.Configuration)
                        .ReadFrom.Services(svc)
                        .Enrich.FromLogContext();
                }
            )
            .ConfigureAppConfiguration(WithApplicationConfiguration)
            .ConfigureServices(WithApplicationServices);

    private static void WithApplicationConfiguration(
        HostBuilderContext context,
        IConfigurationBuilder configurationBuilder
    )
    {
        if (Design.IsDesignMode)
            return;
        configurationBuilder.Sources.Clear();
        configurationBuilder.AddEnvironmentVariables();

        configurationBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .Add<WritableJsonConfigurationSource>(s =>
            {
                s.Path = "appsettings.json";
                s.Optional = false;
                s.ReloadOnChange = true;
                s.FileProvider = null;
                s.ResolveFileProvider();
            })
            .AddJsonFile("appsettings.Development.json", true, true);

        if (context.HostingEnvironment.IsDevelopment())
        {
            configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly());
        }
    }

    private static void WithApplicationServices(
        HostBuilderContext context,
        IServiceCollection services
    )
    {
        services.AddMediator(cfg =>
        {
            cfg.ServiceLifetime = ServiceLifetime.Scoped;
        });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var dbPath = Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ArcadePointsBot.db"
            );
            options.UseSqlite($"Data Source={dbPath}");
        });

        services.AddOptions<TwitchAuthConfig>().BindConfiguration("TwitchAuthConfig");

        services.Configure<TwitchAuthConfig>(options =>
        {
            var rawConfig = context.Configuration;
            options.PropertyChanged += (o, e) =>
            {
                if (e is not PropertyChangedEventArgsEx args)
                    throw new InvalidOperationException();
                rawConfig["TwitchAuthConfig:" + args.PropertyName!] = args.Value?.ToString();
            };
        });

        services.AddSingleton<IObserver<Exception>, GlobalRxExceptionHandler>();
        services.AddSingleton<IAuthenticationService, TwitchAuthenticationService>();
        services.AddScoped<TwitchPointRewardService>();

        services.AddTransient(typeof(IEntityRepository<,>), typeof(DataEntityRepository<,>));
        services.AddTransient<IRewardRepository, RewardRepository>();

        services.AddSingleton<Mouse>();
        services.AddSingleton<Keyboard>();
        services.AddSingleton<TwitchWorker>();

        services.AddTransient<ViewLocator>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<CreateRewardWindowViewModel>();
    }
}
