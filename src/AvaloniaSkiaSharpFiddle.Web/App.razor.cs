using Avalonia.ReactiveUI;
using Avalonia.Web.Blazor;

namespace AvaloniaSkiaSharpFiddle.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        WebAppBuilder.Configure<AvaloniaSkiaSharpFiddle.App>()
            .UseReactiveUI()
            .SetupWithSingleViewLifetime();
    }
}