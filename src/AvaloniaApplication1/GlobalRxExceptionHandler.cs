using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace AvaloniaApplication1
{
    internal class GlobalRxExceptionHandler : IObserver<Exception>
    {
        private readonly ILogger<GlobalRxExceptionHandler> _logger;

        public GlobalRxExceptionHandler(ILogger<GlobalRxExceptionHandler> logger)
        {
            _logger = logger;
        }

        public void OnCompleted()
        {
            if (Debugger.IsAttached) Debugger.Break();
            RxApp.MainThreadScheduler.Schedule(() => { throw new NotImplementedException(); });
        }

        public void OnError(Exception error)
        {
            if (Debugger.IsAttached) Debugger.Break();
            _logger.LogCritical(error, "Something went wrong in the UI");
            RxApp.MainThreadScheduler.Schedule(() => { throw error; });
        }

        public void OnNext(Exception value)
        {
            if (Debugger.IsAttached) Debugger.Break();
            _logger.LogCritical(value, "Something went wrong in the UI");
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                Dispatcher.UIThread.Post(() => lifetime.Shutdown(value.HResult));
            RxApp.MainThreadScheduler.Schedule(() => { throw value; });

        }
    }
}
