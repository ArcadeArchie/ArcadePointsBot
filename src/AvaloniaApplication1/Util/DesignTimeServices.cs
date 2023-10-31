using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Util;

public static class DesignTimeServices
{
    private static IServiceProvider? rawServices;

    private static readonly Lazy<IServiceScope> Scope = new(() =>
    {
        rawServices = Program.ServiceProvider;
        return rawServices.CreateScope();
    });

    public static IServiceProvider Services => Scope.Value.ServiceProvider;
}
