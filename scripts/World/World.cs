using Godot;

public partial class World : Node3D
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("escape"))
        {
            GD.Print("escape was pressed, mosue mode: " + Input.MouseMode);
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }
}
