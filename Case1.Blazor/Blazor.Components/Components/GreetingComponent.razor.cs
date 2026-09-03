using Microsoft.AspNetCore.Components;

namespace Blazor.Components.Components;

public sealed partial class GreetingComponent
{
	private string greeting = string.Empty;

	[Parameter]
	public string Name { get; set; } = string.Empty;

	protected override void OnParametersSet()
	{
		greeting = string.IsNullOrWhiteSpace(Name)
			? "Hello, stranger"
			: $"Hello, {Name}";
	}
}
