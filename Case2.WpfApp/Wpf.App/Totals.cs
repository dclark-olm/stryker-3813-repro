namespace Wpf.App;

// Plain C# with something to mutate, so the run has real mutants of its own.
public sealed class Totals
{
	private readonly int passed;
	private readonly int rejected;

	public Totals(int passed, int rejected)
	{
		this.passed = passed;
		this.rejected = rejected;
	}

	public string Description => Describe();

	public string Describe()
	{
		if (passed + rejected == 0)
			return "Nothing inspected";

		return $"{passed} passed, {rejected} rejected";
	}
}
