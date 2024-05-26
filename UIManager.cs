using Godot;

public partial class UIManager : Control
{
	[Export] public Player player;
	[Export] public Control switchPrompt;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player ??= GetNode<Player>("../Player");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (player.UnlockedTypes.Count > 1 && !switchPrompt.Visible) switchPrompt.Show();
	}
}
