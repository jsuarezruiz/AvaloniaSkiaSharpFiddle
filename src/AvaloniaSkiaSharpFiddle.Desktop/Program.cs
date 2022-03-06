﻿using System;
using Avalonia;

namespace AvaloniaSkiaSharpFiddle
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions()
                {
                    UseDeferredRendering = true
                })
                .With(new X11PlatformOptions()
                {
                    UseDeferredRendering = true
                })
                .With(new AvaloniaNativePlatformOptions()
                {
                    UseDeferredRendering = true
                })
                .LogToTrace()
                .UseSkia();
    }
}