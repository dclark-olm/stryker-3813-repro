using Blazor.Components.Components;
using Bunit;
using Xunit;

namespace Blazor.Components.Tests;

public sealed class TotalsComponentTests : BunitContext
{
	[Fact]
	public void SomethingInspected_ShowsTheCounts()
	{
		IRenderedComponent<TotalsComponent> rendered = Render<TotalsComponent>(parameters => parameters
			.Add(component => component.Passed, 3)
			.Add(component => component.Rejected, 1));

		Assert.Contains("3 passed, 1 rejected", rendered.Markup);
	}

	[Fact]
	public void NothingInspected_ShowsTheEmptyMessage()
	{
		IRenderedComponent<TotalsComponent> rendered = Render<TotalsComponent>(parameters => parameters
			.Add(component => component.Passed, 0)
			.Add(component => component.Rejected, 0));

		Assert.Contains("Nothing inspected", rendered.Markup);
	}
}
