using Godot;

public partial class Dirtpatch : Node3D
{
    enum DirtPatchState
    {
        Dry,
        Watered,
        YoungCactus,
        AgedCactus,
        CactusWithFlowers
    }
    [Export]
    Label3D interactLabel;

    [Export]
    MeshInstance3D groundDirtPatch;

    DirtPatchState currentDirtPatchState = DirtPatchState.Dry;
    CharacterBody3D player;

    MeshInstance3D cactus;

    public override void _Ready()
    {
        player = (CharacterBody3D)GetNode("/root/MainScene/Player");
    }

    public override void _Process(double delta)
    {
        Vector3 positionOfPlayer = player.Position;
        Vector3 positionOfDirtPatch = Position;
        float distanceToPlayer = Position.DistanceTo(positionOfPlayer);
        if (distanceToPlayer < 10f)
        {
            interactLabel.Visible = true;

            if (Input.IsActionJustPressed("interact"))
            {
                switch (currentDirtPatchState)
                {
                    case DirtPatchState.Dry:
                        currentDirtPatchState = DirtPatchState.Watered;
                        interactLabel.Text = "Press F to make it YoungCactus";
                        StandardMaterial3D newMaterial = new StandardMaterial3D();
                        newMaterial.AlbedoTexture = (Texture2D)GD.Load("res://assets/textures/wet_dirt.png");

                        groundDirtPatch.Visible = true;
                        groundDirtPatch.SetSurfaceOverrideMaterial(0, newMaterial);
                        break;
                    case DirtPatchState.Watered:
                        currentDirtPatchState = DirtPatchState.YoungCactus;
                        if (cactus == null)
                        {
                            PackedScene scene = GD.Load<PackedScene>("res://assets/scenes/cactus.tscn");
                            MeshInstance3D instance = scene.Instantiate<MeshInstance3D>();
                            instance.Position = new Vector3(0.0f, 0.5f, 0.0f);
                            AddChild(instance);
                            cactus = instance;
                        }
                        interactLabel.Text = "Press F to make it AgedCactus";
                        break;
                    case DirtPatchState.YoungCactus:
                        currentDirtPatchState = DirtPatchState.AgedCactus;
                        string cactusModelPath = GetPathForCactus(currentDirtPatchState);
                        cactus.Mesh = (Mesh)GD.Load(cactusModelPath);
                        interactLabel.Text = "Press F to make it Cactus Flower";
                        break;
                    case DirtPatchState.AgedCactus:
                        currentDirtPatchState = DirtPatchState.CactusWithFlowers;
                        cactusModelPath = GetPathForCactus(currentDirtPatchState);
                        cactus.Mesh = (Mesh)GD.Load(cactusModelPath);
                        interactLabel.Text = "Congratulions on your first grown cactus! Now sell it";
                        break;
                }
            }
        }
        else
        {
            interactLabel.Visible = false;
        }
    }

    private string GetPathForCactus(DirtPatchState cactusState)
    {
        switch (cactusState)
        {
            case DirtPatchState.YoungCactus:
                return "res://assets/models/nature/cactus/Cactus_3.obj";
            case DirtPatchState.AgedCactus:
                return "res://assets/models/nature/cactus/Cactus_2.obj";
            case DirtPatchState.CactusWithFlowers:
                return "res://assets/models/nature/cactus/CactusFlowers_2.obj";
            default: return "";
        }
    }
}
