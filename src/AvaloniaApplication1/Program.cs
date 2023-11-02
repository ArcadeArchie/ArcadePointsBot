using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using AvaloniaApplication1.Auth;
using AvaloniaApplication1.Config;
using AvaloniaApplication1.Data.Abstractions.Repositories;
using AvaloniaApplication1.Data.Contexts;
using AvaloniaApplication1.Data.Repositories;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AvaloniaApplication1
{
    internal class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static async Task<int> Main(string[] args)
        {
            var host = CreateAppBuilder(args).Build();
            await host.StartAsync();
            ServiceProvider = host.Services;
            var app = BuildAvaloniaAppWithServices(host.Services);
            try
            {
                var res = app.StartWithClassicDesktopLifetime(args);

                await host.StopAsync();

                return res;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Something very bad happened, please send this log to the devs");
                return -1;
            }
            //BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }


        public static IHostBuilder CreateAppBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
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
            //if (context.HostingEnvironment.IsDevelopment())
            //{
            //    services.AddDbContext<ApplicationDbContext>(options =>
            //    {
            //        var connString = "Server=192.168.178.28; User ID=root; Password=31401577; Database=twitchTest";
            //        options.UseMySql(connString, ServerVersion.AutoDetect(connString));
            //    });
            //}
            //else
            //{
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var dbPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArcadePointsBot.db");
                options.UseSqlite($"Data Source={dbPath}");
            });
            //}

            //services.Configure<TwitchAuthConfig>(section);
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
            services.AddSingleton<ViewLocator>();
            services.AddSingleton<IAuthenticationService, TwitchAuthenticationService>();
            services.AddScoped<TwitchPointRewardService>();

            services.AddScoped(typeof(IEntityRepository<,>), typeof(DataEntityRepository<,>));
            services.AddScoped<IRewardRepository, RewardRepository>();

            services.AddSingleton<TwitchWorker>();

            services.AddScoped<CreateRewardWindowViewModel>();
            services.AddScoped<MainWindowViewModel>();
        }

        public static AppBuilder BuildAvaloniaAppWithServices(IServiceProvider services) => AppBuilder
            .Configure(() => new App(services))
            .UsePlatformDetect()
            .UseReactiveUI()
            .WithInterFont()
            .LogToTrace();
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            var host = Host.CreateDefaultBuilder().ConfigureServices(WithApplicationServices).Build();
            Program.ServiceProvider = host.Services;
            return BuildAvaloniaAppWithServices(host.Services);
        }
    }
}
