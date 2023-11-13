using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace ArcadePointsBot.ViewModels
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
        
        [ObservableAsProperty]
        public bool HasError { get; }

        public ObservableCollection<Error> Errors { get; } = new();

        protected ViewModelBase()
        {
            IsBusyObservable = this.WhenAnyValue(vm => vm.IsBusy, isBusy => !isBusy);
            Errors
                .ToObservableChangeSet(x => x)
                .ToCollection()
                .Select(x => x.Any())
                .ToPropertyEx(this, x => x.HasError);
        }
    }
}
/*
 

        [ObservableAsProperty]
        public bool HasActions { get; }
            Actions.ToObservableChangeSet(x => x).ToCollection().Select(x => x.Any()).ToPropertyEx(this, x => x.HasActions);
 
 */