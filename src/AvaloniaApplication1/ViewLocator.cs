using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaApplication1.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AvaloniaApplication1
{
    public class ViewLocator : IDataTemplate
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Control? Build(object? data)
        {
            if(data is null)
                return new TextBlock { Text = "Not Found" };

            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                var scope = _serviceProvider.CreateScope();
                return (Control)scope.ServiceProvider.GetRequiredService(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}