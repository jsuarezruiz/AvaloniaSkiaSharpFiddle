#nullable disable
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaSkiaSharpFiddle.Converters
{
    public class ModeToStatusBarTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Mode mode)
            {
                switch (mode)
                {
                    case Mode.Working:
                        return "Compiling drawing code...";
                    case Mode.Error:
                        return "Some errors were found.";
                }
            }

            return "Ready.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}