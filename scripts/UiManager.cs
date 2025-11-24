using Godot;

public partial class UiManager : Control
{
	public static UiManager Instance;

	GameManager ObjectiveManager;

	[Export]
	public Label CurrentObjectiveLabel { get; set; }

	[Export]
	public Label InteractLabel { get; set; }

	[Export]
	public Label CoinsLabel { get; set; }

	public override void _Ready()
	{
		Instance = this;
	}
}
