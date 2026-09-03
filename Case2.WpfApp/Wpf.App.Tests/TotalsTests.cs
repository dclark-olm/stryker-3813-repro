using Wpf.App;
using Xunit;

namespace Wpf.AppTests;

public sealed class TotalsTests
{
	[Fact]
	public void SomethingInspected_DescribesTheCounts()
	{
		string description = new Totals(3, 1).Describe();

		Assert.Equal("3 passed, 1 rejected", description);
	}

	[Fact]
	public void NothingInspected_DescribesTheEmptyCase()
	{
		string description = new Totals(0, 0).Describe();

		Assert.Equal("Nothing inspected", description);
	}
}
