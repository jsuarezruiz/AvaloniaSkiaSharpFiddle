#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace AvaloniaSkiaSharpFiddle.Controls
{
    public class SkiaCanvas : Control
    {
        CustomDrawOp _customDrawOp;

        public SkiaCanvas()
        {
            ClipToBounds = true;
        }

        public SKSizeI DrawingSize { get; set; }
        public SKImage RasterDrawing { get; set; }

        class CustomDrawOp : ICustomDrawOperation
        {
            static readonly SKColor PaneColor = 0xFFF5F5F5;
            static readonly SKColor AlternatePaneColor = 0xFFF0F0F0;
            const int BaseBlockSize = 8;

            public CustomDrawOp(Rect bounds, SKSizeI drawingSize, SKImage rasterDrawing)
            {
                Bounds = bounds;
                DrawingSize = drawingSize;
                RasterDrawing = rasterDrawing;
            }

            public void Dispose()
            {
                // No-op in this example
            }

            public SKSizeI DrawingSize { get; set; }
            public SKImage RasterDrawing { get; set; }
            public Rect Bounds { get; }
            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation other) => false;

            public void Render(IDrawingContextImpl context)
            {
                var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

                canvas.Clear(PaneColor);

                canvas.ClipRect(SKRect.Create(DrawingSize));

                double height = Bounds.Height;
                double width = Bounds.Width;

                DrawTransparencyBackground(canvas, width, height);

                if (RasterDrawing != null)
                    canvas.DrawImage(RasterDrawing, 0, 0);
            }

            void DrawTransparencyBackground(SKCanvas canvas, double width, double height)
            {
                var blockSize = BaseBlockSize;

                var offsetMatrix = SKMatrix.CreateScale(2 * blockSize, blockSize);
                var skewMatrix = SKMatrix.CreateSkew(0.5f, 0);
#pragma warning disable CS0618 // Type or member is obsolete
                SKMatrix.PreConcat(ref offsetMatrix, ref skewMatrix);
#pragma warning restore CS0618 // Type or member is obsolete

                using (var path = new SKPath())
                using (var paint = new SKPaint())
                {
                    path.AddRect(SKRect.Create(blockSize / -2, blockSize / -2, blockSize, blockSize));

                    paint.PathEffect = SKPathEffect.Create2DPath(offsetMatrix, path);
                    paint.Color = AlternatePaneColor;

                    canvas.DrawRect(SKRect.Create((float)width + blockSize, (float)height + blockSize), paint);
                }
            }
        }

        public override void Render(DrawingContext context)
        {
            _customDrawOp = new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), DrawingSize, RasterDrawing);
            context.Custom(_customDrawOp);
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }


    }
}