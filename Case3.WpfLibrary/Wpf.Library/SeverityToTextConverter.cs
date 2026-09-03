using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Library;

public sealed class SeverityToTextConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		int severity = value is int level ? level : 0;

		if (severity >= 3)
			return "Critical";

		return severity == 0 ? "Clear" : "Warning";
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
