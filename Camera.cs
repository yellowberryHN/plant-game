using Godot;
using System;

public partial class Camera : Node3D
{
	private float yaw = 0f;

	private float pitch = 0f;

	private float yawSensitivity = .002f;

	private float pitchSensitivity = .002f;

	[Export] public Node3D CameraTarget;
	[Export] public float pitchMax = 50;
	[Export] public float pitchMin = -50;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMotion && Input.MouseMode != 0)
		{
			yaw += -eventMotion.Relative.X * yawSensitivity;
			pitch += eventMotion.Relative.Y * pitchSensitivity;
		}
		base._Input(@event);
	}
	
	// TODO: make this only fire when movement changed
	public override void _PhysicsProcess(double delta)
	{
		Vector3 newRotation = CameraTarget.Rotation;
		newRotation.Y = Mathf.Lerp(CameraTarget.Rotation.Y, yaw, (float)delta * 10);
		newRotation.X = Mathf.Lerp(CameraTarget.Rotation.X, pitch, (float)delta * 10);
		CameraTarget.Rotation = newRotation;

		pitch = Mathf.Clamp(pitch, Mathf.DegToRad(pitchMin), Mathf.DegToRad(pitchMax));

		float cameraInputX = Input.GetAxis("look_right", "look_left");
		float cameraInputY = Input.GetAxis("look_down", "look_up");
		Vector2 cameraInput = new Vector2(cameraInputX, cameraInputY);

		yaw += cameraInput.X * yawSensitivity * 30;
		pitch += cameraInput.Y * pitchSensitivity * 20;
		
		base._PhysicsProcess(delta);
	}
}
