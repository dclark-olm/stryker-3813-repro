using System.Globalization;
using Wpf.Library;
using Xunit;

namespace Wpf.LibraryTests;

public sealed class SeverityToTextConverterTests
{
	[Theory]
	[InlineData(0, "Clear")]
	[InlineData(1, "Warning")]
	[InlineData(3, "Critical")]
	public void Severity_ConvertsToText(int severity, string expected)
	{
		object converted = new SeverityToTextConverter().Convert(severity, typeof(string), null, CultureInfo.InvariantCulture);

		Assert.Equal(expected, converted);
	}
}
