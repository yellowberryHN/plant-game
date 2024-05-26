using Godot;

public partial class Main : Node3D
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("quit_game"))
		{
			GetTree().Quit();
		}
	}
}
