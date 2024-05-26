using Godot;

public partial class Key : Node3D
{
	[Export] public Node3D DestructionTarget;

	public void OnBodyEntered(Node3D node)
	{
		GD.Print("got the key!");
		
		Label unlockLabel = GetNode<Label>("../UI/UnlockLabel");
		unlockLabel.Text = $"You got the key! Go back home!";
		unlockLabel.Show();
			
		Timer timer = unlockLabel.GetNode<Timer>("Timer");
		timer.Start();
		
		DestructionTarget.QueueFree();
		QueueFree();
	}
}
