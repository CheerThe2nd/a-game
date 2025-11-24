using Godot;

public partial class GrowPlot : Node3D
{
	[Signal]
	public delegate void PlayerInRangeEventHandler(bool inRange);

	[Signal]
	public delegate void PlantFullyGrownEventHandler();

	enum GrowPlotState
	{
		Dry,
		Watered,
		HasPlant
	}

	enum PlantType
	{
		Cactus,
	}

	enum PlantState
	{
		YoungPlant,
		AgedPlant,
		ReadyToHarvest,
	}

	[Export]
	MeshInstance3D groundMesh;
	[Export]
	Area3D area3D;
	[Export]
	MeshInstance3D plantModel;

	PlantState plantState;
	GrowPlotState growPlotState = GrowPlotState.Dry;

	bool playerInRange;

	public override void _Ready()
	{
		area3D.BodyEntered += OnBodyEntered;
		area3D.BodyExited += OnBodyExit;
	}

	public override void _Process(double delta)
	{
		if (!playerInRange) return;
		HandleInteractionWithGrowPlot();
	}

	private void OnBodyEntered(Node3D body)
	{
		if (!body.IsInGroup("player")) return;
		GD.Print($"body has entered dirtpatch range! {body}");
		playerInRange = true;
		UiManager.Instance.InteractLabel.Visible = true;
		UiManager.Instance.InteractLabel.Text = GetTextForInteractLabel(growPlotState);
	}

	private void OnBodyExit(Node3D body)
	{
		if (!body.IsInGroup("player")) return;
		GD.Print("player has exited dirtpatch range!");
		playerInRange = false;
		UiManager.Instance.InteractLabel.Visible = false;
	}

	private void HandleInteractionWithGrowPlot()
	{
		if (!Input.IsActionJustPressed("interact")) return;

		// for now we always plant a cactus
		switch (growPlotState)
		{
			case GrowPlotState.Dry:
				growPlotState = GrowPlotState.Watered;

				StandardMaterial3D newMaterial = new StandardMaterial3D();
				newMaterial.AlbedoTexture = (Texture2D)GD.Load("res://assets/textures/wet_dirt.png");
				groundMesh.SetSurfaceOverrideMaterial(0, newMaterial);

				UiManager.Instance.InteractLabel.Text = "Press (F) to plant a Young Cactus";

				return;
			case GrowPlotState.Watered:
				growPlotState = GrowPlotState.HasPlant;
				plantState = PlantState.YoungPlant;

				string cactusModelPath = GetModelPathForCactusByPlantState(plantState);
				plantModel.Mesh = GD.Load<Mesh>(cactusModelPath);

				string interactLabelText = "Press (F) to make this Young Plant an Aged Plant";
				UiManager.Instance.InteractLabel.Text = interactLabelText;

				return;
		}

		if (growPlotState != GrowPlotState.HasPlant) return;

		switch (plantState)
		{
			case PlantState.YoungPlant:
				plantState = PlantState.AgedPlant;

				string plantModelPath = GetModelPathForCactusByPlantState(plantState);
				plantModel.Mesh = (Mesh)GD.Load(plantModelPath);

				UiManager.Instance.InteractLabel.Text = "Press (F) to make this plant ready to harvest";
				break;
			case PlantState.AgedPlant:
				plantState = PlantState.ReadyToHarvest;

				plantModelPath = GetModelPathForCactusByPlantState(plantState);
				plantModel.Mesh = (Mesh)GD.Load(plantModelPath);

				UiManager.Instance.InteractLabel.Text = "Press (F) to harvest this plant";
				break;
			case PlantState.ReadyToHarvest:
				// TODO: remove the plant and add it to users inventory
				GD.Print("TODO: remove the plant and add it to users inventory");
				break;
		}
	}

	private static string GetModelPathForCactusByPlantState(PlantState plantState)
	{
		switch (plantState)
		{
			case PlantState.YoungPlant:
				return "res://assets/models/nature/cactus/Cactus_3.obj";
			case PlantState.AgedPlant:
				return "res://assets/models/nature/cactus/Cactus_2.obj";
			case PlantState.ReadyToHarvest:
				return "res://assets/models/nature/cactus/CactusFlowers_2.obj";
			default: return "";
		}
	}

	private static string GetTextForInteractLabel(GrowPlotState growPlotState)
	{
		// for now we always plant a cactus
		switch (growPlotState)
		{
			case GrowPlotState.Dry: return "Press (F) to water this grow plot";
			case GrowPlotState.Watered: return "Press (F) to plant a young cactus";
			case GrowPlotState.HasPlant: return "Press (F) to harvest the plant";
			default: return "";
		}
	}

}
