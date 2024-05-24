using Godot;
using System;

public partial class Camera : Marker3D
{
	private float CameraRotH;
	private float CameraRotV;

	private Node3D CameraH;
	private Node3D CameraV;

	[Export]
	private float CameraTiltMin = -55.0f;
	[Export]
	private float CameraTiltMax = 75.0f;
	
	[Export]
	private float SensitivityH = 0.01f;
	[Export]
	private float SensitivityV = 0.01f;
	
	[Export]
	private float AccelerationH = 5.0f;
	[Export]
	private float AccelerationV = 10.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CameraH = GetNode<Node3D>("H");
		CameraV = GetNode<Node3D>("H/V");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMotion)
		{
			CameraRotH += -eventMotion.Relative.X;
			CameraRotV += eventMotion.Relative.Y;

			CameraRotV = Mathf.Clamp(CameraRotV, CameraTiltMin, CameraTiltMax);
		}
		base._Input(@event);
	}
	
	// TODO: make this only fire when movement changed
	public override void _PhysicsProcess(double delta)
	{
		var new_rot_h = CameraH.RotationDegrees;
		new_rot_h.Y = (float)Mathf.Lerp(new_rot_h.Y, CameraRotH, delta * AccelerationH);
		CameraH.RotationDegrees = new_rot_h;
			
		var new_rot_v = CameraH.RotationDegrees;
		new_rot_v.X = (float)Mathf.Lerp(new_rot_v.X, CameraRotV, delta * AccelerationV);;
		CameraV.RotationDegrees = new_rot_v;
		
		base._PhysicsProcess(delta);
	}
}
