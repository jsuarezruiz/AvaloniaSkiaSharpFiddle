#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaSkiaSharpFiddle.Controls;
using AvaloniaSkiaSharpFiddle.ViewModels;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace AvaloniaSkiaSharpFiddle
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            var editor = this.FindControl<TextEditor>("editor");

            if (editor != null)
            {
                editor.TextArea.TextView.LineTransformers.Add(new CompilationResultsTransformer(ViewModel));
                editor.TextArea.TextView.CurrentLineBackground = new SolidColorBrush(Colors.Transparent);
                editor.TextArea.TextView.CurrentLineBorder = new Pen(new SolidColorBrush(Color.FromRgb(234, 234, 234)), 2);

                Observable.FromEventPattern(editor, nameof(editor.TextChanged))
                    .Select(evt => (evt.Sender as TextEditor)?.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Throttle(TimeSpan.FromMilliseconds(250))
                    .DistinctUntilChanged()
                    .Subscribe(source => Dispatcher.UIThread.InvokeAsync(new Action(() => ViewModel.SourceCode = source)));

                _ = LoadInitialSourceAsync();
            }

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                ViewModel.CompilationMessages.CollectionChanged += OnCompilationMessagesChanged;
            }
        }

        public MainViewModel ViewModel => DataContext as MainViewModel;

        void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        async Task LoadInitialSourceAsync()
        {
            var type = typeof(MainWindow);
            var assembly = type.Assembly;

            var resource = $"{type.Namespace}.Resources.InitialSource.cs";

            using (var stream = assembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream))
            {
                var editor = this.FindControl<TextEditor>("editor");

                if (editor != null)
                {
                    editor.Text = await reader.ReadToEndAsync();
                }
            }
        }

        void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.RasterDrawing) ||
                e.PropertyName == nameof(MainViewModel.GpuDrawing))
            {
                var preview = this.FindControl<SkiaCanvas>("preview");

                if (preview != null)
                {
                    preview.DrawingSize = ViewModel.DrawingSize;
                    preview.RasterDrawing = ViewModel.RasterDrawing;
                    preview.InvalidateVisual();
                }
            }
            else if (e.PropertyName == nameof(MainViewModel.DrawingSize))
            {
                var preview = this.FindControl<SkiaCanvas>("preview");

                if (preview != null)
                {
                    preview.DrawingSize = ViewModel.DrawingSize;
                }
            }
            else if (e.PropertyName == nameof(MainViewModel.RasterDrawing))
            {
                var preview = this.FindControl<SkiaCanvas>("preview");

                if (preview != null)
                {
                    preview.RasterDrawing = ViewModel.RasterDrawing;
                }
            }
        }

        void OnCompilationMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var editor = this.FindControl<TextEditor>("editor");

            if (editor != null)
            {
                editor.TextArea.TextView.Redraw();
            }
        }
    }
}