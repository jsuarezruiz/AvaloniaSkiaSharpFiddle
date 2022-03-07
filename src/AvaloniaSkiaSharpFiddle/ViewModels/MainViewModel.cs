#nullable disable
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using SkiaSharp;

namespace AvaloniaSkiaSharpFiddle.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        readonly Compiler compiler = new Compiler();

        string sourceCode;

        int drawingWidth = 256;
        int drawingHeight = 256;

        ColorCombination colorCombination;

        SKImage rasterDrawing;
        SKImage gpuDrawing;

        Mode mode = Mode.Ready;

        CancellationTokenSource cancellation;
        CompilationResult lastResult;

        public MainViewModel()
        {
            var color = SKImageInfo.PlatformColorType;
            var colorString = color == SKColorType.Bgra8888 ? "BGRA" : "RGBA";

            ColorCombinations = new ColorCombination[]
            {
                new ColorCombination(colorString, color, null),
                new ColorCombination($"{colorString} (sRGB)", color, SKColorSpace.CreateSrgb()),
                new ColorCombination("F16 (sRGB Linear)", SKColorType.RgbaF16, SKColorSpace.CreateSrgbLinear()),
            };

            ColorCombination = ColorCombinations[0];

            CompilationMessages = new ObservableCollection<CompilationMessage>();

            var skiaAss = typeof(SKSurface).Assembly;
            if (skiaAss.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute informational)
                SkiaSharpVersion = informational.InformationalVersion;
            else if (skiaAss.GetCustomAttribute(typeof(AssemblyFileVersionAttribute)) is AssemblyFileVersionAttribute fileVersion)
                SkiaSharpVersion = fileVersion.Version;
            else if (skiaAss.GetCustomAttribute(typeof(AssemblyVersionAttribute)) is AssemblyVersionAttribute version)
                SkiaSharpVersion = version.Version;
            else
                SkiaSharpVersion = "<unknown>";
        }

        public ColorCombination[] ColorCombinations { get; }

        public SKSizeI DrawingSize => new SKSizeI(DrawingWidth, DrawingHeight);

        public SKImageInfo ImageInfo => new SKImageInfo(DrawingWidth, DrawingHeight);

        public ObservableCollection<CompilationMessage> CompilationMessages { get; }

        public string SkiaSharpVersion { get; }

        public string SourceCode
        {
            get => sourceCode;
            set => SetProperty(ref sourceCode, value, onChanged: OnSourceCodeChanged);
        }

        public int DrawingWidth
        {
            get => drawingWidth;
            set => SetProperty(ref drawingWidth, value, onChanged: OnDrawingSizeChanged);
        }

        public int DrawingHeight
        {
            get => drawingHeight;
            set => SetProperty(ref drawingHeight, value, onChanged: OnDrawingSizeChanged);
        }

        public ColorCombination ColorCombination
        {
            get => colorCombination;
            set => SetProperty(ref colorCombination, value, onChanged: OnColorCombinationChanged);
        }

        public SKImage RasterDrawing
        {
            get => rasterDrawing;
            private set => SetProperty(ref rasterDrawing, value);
        }

        public SKImage GpuDrawing
        {
            get => gpuDrawing;
            private set => SetProperty(ref gpuDrawing, value);
        }

        public Mode Mode
        {
            get => mode;
            private set => SetProperty(ref mode, value);
        }

        void OnDrawingSizeChanged()
        {
            OnPropertyChanged(nameof(DrawingSize));
            OnPropertyChanged(nameof(ImageInfo));

            GenerateDrawings();
        }

        async void OnSourceCodeChanged()
        {
            cancellation?.Cancel();
            cancellation = new CancellationTokenSource();

            Mode = Mode.Working;

            try
            {
                lastResult = await compiler.CompileAsync(SourceCode, cancellation.Token);
                foreach (var compilationMessage in lastResult.CompilationMessages)
                    CompilationMessages.Add(compilationMessage);

                Mode = lastResult.HasErrors ? Mode.Error : Mode.Ready;
            }
            catch (OperationCanceledException)
            {
            }

            GenerateDrawings();
        }

        void OnColorCombinationChanged()
        {
            GenerateDrawings();
        }

        void GenerateDrawings()
        {
            GenerateRasterDrawing();
            GenerateGpuDrawing(); 
            
            OnPropertyChanged(nameof(GpuDrawing));
        }

        void GenerateRasterDrawing()
        {
            var old = RasterDrawing;

            var info = ImageInfo;
            using (var surface = SKSurface.Create(info))
            {
                Draw(surface, info);
                RasterDrawing = surface.Snapshot();
            }

            old?.Dispose();
        }

        void GenerateGpuDrawing()
        {
            // TODO: Implement offscreen GPU drawing.
        }

        void Draw(SKSurface surface, SKImageInfo info)
        {
            var messages = lastResult?.Draw(surface, info.Size);

            if (messages?.Any() == true)
            {
                foreach (var message in messages)
                    CompilationMessages.Add(message);
            }
        }
    }
}