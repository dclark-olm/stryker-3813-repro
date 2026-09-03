namespace Wpf.Library.Views;

/// <summary>Interaction logic for Alarm.xaml.</summary>
public sealed partial class Alarm
{
	// InitializeComponent is declared by the generated half of this partial class. CS0103 is
	// reported here when that generated half is missing from the compilation.
	public Alarm()
	{
		InitializeComponent();
		Message.Text = Describe(0);
	}

	// Something for Stryker to mutate in the hand-written half of the partial class.
	public static string Describe(int severity) => severity > 0 ? "Alarm raised" : "No alarm";
}
