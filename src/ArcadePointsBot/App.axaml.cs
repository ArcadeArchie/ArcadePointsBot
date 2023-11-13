using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ArcadePointsBot.Auth;
using ArcadePointsBot.Config;
using ArcadePointsBot.Data.Abstractions.Repositories;
using ArcadePointsBot.Data.Contexts;
using ArcadePointsBot.Data.Repositories;
using ArcadePointsBot.Services;
using ArcadePointsBot.ViewModels;
using ArcadePointsBot.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ArcadePointsBot;
public partial class App : Application
{
    public IHost? GlobalHost { get; private set; }

    private void EnsureDb(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("App");
        logger.LogInformation("Checking for Db updates");
        try
        {
            using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrations = db.Database.GetPendingMigrations();
            if (migrations.Any())
            {
                logger.LogInformation("Found {count} updates, updating DB", migrations.Count());
                db.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An Error occurred updating the DB");
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
        EnsureDb(GlobalHost.Services);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            desktop.MainWindow = new MainWindow
            {
                DataContext = GlobalHost.Services.GetRequiredService<MainWindowViewModel>(),
            };
            desktop.Exit += (_, _) =>
            {
                GlobalHost.StopAsync().GetAwaiter().GetResult();
                GlobalHost.Dispose();
                GlobalHost = null;
            };
        }
        
        DataTemplates.Add(GlobalHost.Services.GetRequiredService<ViewLocator>());

        base.OnFrameworkInitializationCompleted();

        await GlobalHost.StartAsync();
    }

    
        public static IHostBuilder CreateAppBuilder() => Host
        .CreateDefaultBuilder(Environment.GetCommandLineArgs())
        .UseSerilog((ctx, svc, cfg) =>
        {
            cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(svc)
            .Enrich.FromLogContext();
        })
        .ConfigureAppConfiguration(WithApplicationConfiguration)
        .ConfigureServices(WithApplicationServices);

        private static void WithApplicationConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            if (Design.IsDesignMode)
                return;
            configurationBuilder.Sources.Clear();
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

            configurationBuilder.AddEnvironmentVariables();
        }
        private static void WithApplicationServices(HostBuilderContext context, IServiceCollection services)
        {            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var dbPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArcadePointsBot.db");
                options.UseSqlite($"Data Source={dbPath}");
            });

            services.AddOptions<TwitchAuthConfig>().BindConfiguration("TwitchAuthConfig");

            services.Configure<TwitchAuthConfig>(options =>
            {
                var rawConfig = context.Configuration;
                options.PropertyChanged += (o, e) =>
                {
                    if (e is not PropertyChangedEventArgsEx args) throw new InvalidOperationException();
                    rawConfig["TwitchAuthConfig:" + args.PropertyName!] = args.Value?.ToString();
                };
            });

            services.AddSingleton<IObserver<Exception>, GlobalRxExceptionHandler>();
            services.AddSingleton<IAuthenticationService, TwitchAuthenticationService>();
            services.AddScoped<TwitchPointRewardService>();

            services.AddScoped(typeof(IEntityRepository<,>), typeof(DataEntityRepository<,>));
            services.AddScoped<IRewardRepository, RewardRepository>();

            services.AddSingleton<TwitchWorker>();

            services.AddTransient<ViewLocator>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<CreateRewardWindowViewModel>();
        }
}