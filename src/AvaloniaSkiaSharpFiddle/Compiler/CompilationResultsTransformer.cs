using Avalonia.Media;
using AvaloniaEdit.Rendering;
using AvaloniaSkiaSharpFiddle.ViewModels;

namespace AvaloniaSkiaSharpFiddle
{
    public class CompilationResultsTransformer : ColorizingTransformer
    {
        private static readonly TextDecorationCollection errorTextDecorations;
        private static readonly TextDecorationCollection warningTextDecorations;

        static CompilationResultsTransformer()
        {
            errorTextDecorations = new TextDecorationCollection
            {
                new TextDecoration
                {
                    Location = TextDecorationLocation.Underline,
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 2
                }
            };
            warningTextDecorations = new TextDecorationCollection
            {
                new TextDecoration
                {
                    Location = TextDecorationLocation.Underline,
                     Stroke = new SolidColorBrush(Colors.Green),
                    StrokeThickness = 1
                }
            };
        }

        public CompilationResultsTransformer(MainViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public MainViewModel ViewModel { get; }

        protected override void Colorize(ITextRunConstructionContext context)
        {
            var lineStart = context.VisualLine.StartOffset;
            var lineLength = context.VisualLine.VisualLengthWithEndOfLineMarker;
            var lineEnd = lineStart + lineLength;

            foreach (var message in ViewModel.CompilationMessages)
            {
                var spanStart = message.StartOffset;
                var spanEnd = message.EndOffset;

                if (spanEnd < lineStart || spanStart > lineEnd)
                    continue;

                if (spanStart < lineStart)
                    spanStart = lineStart;
                if (spanEnd > lineEnd)
                    spanEnd = lineEnd;

                var startColumn = context.VisualLine.GetVisualColumn(spanStart - lineStart);
                var endColumn = context.VisualLine.GetVisualColumn(spanEnd - lineStart);

                if (spanStart == spanEnd)
                {
                    if (endColumn < lineLength)
                        endColumn++;
                    else if (startColumn > 0)
                        startColumn--;
                }

                ChangeVisualElements(startColumn, endColumn, element =>
                {
                    var decorations = message.IsError ? errorTextDecorations : warningTextDecorations;
                    // TODO:
                    //element.TextRunProperties.SetTextDecorations(decorations);
                });
            }
        }
    }
}
