using Avalonia;
using SkiaSharp;
using System;

namespace AvaloniaSkiaSharpFiddle.Models
{
    public class SKPaintEventArgs : EventArgs
    {
        public SKCanvas Canvas { get; set; }
        public Rect Bounds { get; set; }
    }
}
