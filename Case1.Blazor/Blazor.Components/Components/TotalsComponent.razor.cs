using Microsoft.AspNetCore.Components;

namespace Blazor.Components.Components;

public sealed partial class TotalsComponent
{
	private string summary = string.Empty;

	private string title = string.Empty;

	[Parameter]
	public int Passed { get; set; }

	[Parameter]
	public int Rejected { get; set; }

	// The only override in this file is what CS0115 is reported against: the base class
	// ComponentBase is declared by the generated half of this partial class, not here.
	protected override void OnInitialized()
	{
		title = "Totals";
		summary = Passed + Rejected > 0
			? $"{Passed} passed, {Rejected} rejected"
			: "Nothing inspected";
	}
}
