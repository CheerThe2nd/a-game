using Godot;

public partial class World : Node3D
{
	public override void _Process(double delta)
	{
		// TODO: move this somewhere else
		if (Input.IsActionJustPressed("escape"))
		{
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
