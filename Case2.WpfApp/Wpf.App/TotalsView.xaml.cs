namespace Wpf.App;

public sealed partial class TotalsView
{
	public TotalsView()
	{
		InitializeComponent();
		Description.Text = new Totals(3, 1).Describe();
	}
}
