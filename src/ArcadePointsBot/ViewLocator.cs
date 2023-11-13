using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaApplication1.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvaloniaApplication1
{
    public static class ViewLocatorHelpers
    {
        public static IServiceCollection AddView<TViewModel, TView>(this IServiceCollection services)
            where TView : Control, new()
            where TViewModel : ViewModelBase
        {
            services.AddSingleton(new ViewLocator.ViewLocationDescriptor(typeof(TViewModel), () => new TView()));
            return services;
        }
    }
    public class ViewLocator : IDataTemplate
    {
        private readonly Dictionary<Type, Func<Control>> _views;

        public ViewLocator(IEnumerable<ViewLocationDescriptor> descriptors)
        {
            _views = descriptors.ToDictionary(x => x.ViewModel, x => x.Factory);
        }

        public Control Build(object? param) => _views[param!.GetType()]();
        public bool Match(object? param) => param is not null && _views.ContainsKey(param.GetType());
        public record ViewLocationDescriptor(Type ViewModel, Func<Control> Factory);
    }
}