using Avalonia.Threading;
using ReactiveUI;
using System;

namespace AvaloniaApplication1.ViewModels
{
    public partial class ViewModelBase : ReactiveObject
    {
        protected IObservable<bool> IsBusyObservable { get; init; }

        private string? _statusText;
        public string? StatusText
        {
            get => _statusText;
            set => Dispatcher.UIThread.Post(() => this.RaiseAndSetIfChanged(ref _statusText, value));
        }
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => Dispatcher.UIThread.Post(() => this.RaiseAndSetIfChanged(ref _isBusy, value));
        }
        public bool HasError { get; set; }


        protected ViewModelBase()
        {
            IsBusyObservable = this.WhenAnyValue(vm => vm.IsBusy, isBusy => !isBusy);
        }
    }
}
