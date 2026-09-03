using Blazor.Components.Components;
using Bunit;
using Xunit;

namespace Blazor.Components.Tests;

public sealed class GreetingComponentTests : BunitContext
{
	[Fact]
	public void NameSupplied_GreetsByName()
	{
		IRenderedComponent<GreetingComponent> rendered = Render<GreetingComponent>(parameters => parameters
			.Add(component => component.Name, "Ada"));

		Assert.Contains("Hello, Ada", rendered.Markup);
	}

	[Fact]
	public void NoName_GreetsTheStranger()
	{
		IRenderedComponent<GreetingComponent> rendered = Render<GreetingComponent>(parameters => parameters
			.Add(component => component.Name, string.Empty));

		Assert.Contains("Hello, stranger", rendered.Markup);
	}
}
