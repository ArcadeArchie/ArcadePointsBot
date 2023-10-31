using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using AvaloniaApplication1.Auth;
using AvaloniaApplication1.Config;
using AvaloniaApplication1.Data.Abstractions.Repositories;
using AvaloniaApplication1.Data.Contexts;
using AvaloniaApplication1.Data.Repositories;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.Util;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvaloniaApplication1;
public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services;
        if (!Design.IsDesignMode)
            EnsureDb();
        RxApp.DefaultExceptionHandler = _services.GetRequiredService<IObserver<Exception>>();
    }

    private void EnsureDb()
    {
        using var scope = _services.CreateScope();
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
        AvaloniaXamlLoader.Load(_services, this);

        Resources[typeof(IServiceProvider)] = _services;
        DataTemplates.Add(_services.GetRequiredService<ViewLocator>());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            desktop.MainWindow = new MainWindow
            {
                DataContext = this.CreateInstance<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}