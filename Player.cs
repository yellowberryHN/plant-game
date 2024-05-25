using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = Godot.Vector2;
using Vector3 = Godot.Vector3;

public partial class Player : CharacterBody3D
{	
	public float Speed = 5.0f;
	public float JumpVelocity = 4.5f;
	private float turnSpeed = 12f;
	
	[Export]
	public PlantType current_type = PlantType.Normal;

	[Export] public Node3D CameraTarget;
	[Export] public Node3D CameraParent;

	private float cameraTilt = 0f;
	private float cameraSpeed = 0f;
	
	public HashSet<PlantType> UnlockedTypes { get; set; } = new() { PlantType.Normal };

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	
	private Dictionary<PlantType, Node3D> plant_models = new();
	private int type_index = 0;
	
	private const float DEFAULT_SPEED = 15.0f;
	private const float DEFAULT_JUMP_VELOCITY = 9.0f;
	
	public override void _PhysicsProcess(double delta)
	{
		CameraSmoothFollow(delta);
		Vector3 velocity = Velocity;
		Vector3 rotation = Rotation;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle floor logic
		if (IsOnFloor())
		{
			if (Input.IsActionJustPressed("swap"))
				CycleType();
			if (Input.IsActionJustPressed("jump") && CanJump()) 
				velocity.Y = JumpVelocity;
		}
		
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector(
			"move_left",
			"move_right",
			"move_forward",
			"move_back"
		);

		cameraTilt = CameraTarget.GlobalTransform.Basis.GetEuler().Y;
		
		Vector3 direction = (new Vector3(inputDir.X, 0, inputDir.Y).Rotated(Vector3.Up, cameraTilt)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			rotation.Y = Mathf.LerpAngle(rotation.Y, Mathf.Atan2(-velocity.X, -velocity.Z), turnSpeed * (float)delta);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}
		
		Rotation = rotation;
		
		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Ready()
	{
		Speed = DEFAULT_SPEED;
		JumpVelocity = DEFAULT_JUMP_VELOCITY;
		
		foreach (var plant in GetNode("Pot/Plant").GetChildren())
		{
			plant_models.Add(Enum.Parse<PlantType>(plant.Name), GetNode<Node3D>($"Pot/Plant/{plant.Name}"));
		}
		SetPlant(current_type);
		
		// for debugging
		UnlockNewType(PlantType.BerryBush);
		UnlockNewType(PlantType.Mushroom);

		base._Ready();
	}

	private bool CanJump()
	{
		switch (current_type)
		{
			case PlantType.Flower:
				return false;
			default:
				return true;
		}
	}
	
	private void CameraSmoothFollow(double delta)
	{
		Vector3 cameraOffset = new Vector3(0, 1.5f, 0).Rotated(Vector3.Up, cameraTilt);
		cameraSpeed = 250;
		float cameraTimer = Mathf.Clamp((float)delta * cameraSpeed / 20, 0, 1);
		Transform3D newTransform = CameraParent.GlobalTransform;
		newTransform.Origin = newTransform.Origin.Lerp(this.GlobalTransform.Origin + cameraOffset, cameraTimer);
		CameraParent.GlobalTransform = newTransform;
	}

	private void CycleType()
	{
		type_index = (type_index + 1) % Enum.GetValues(typeof(PlantType)).Length;
		PlantType newType = (PlantType) type_index;
		SetPlant(newType);
	}

	private void SetPlant(PlantType plant)
	{
		if (UnlockedTypes.Contains(plant))
		{
			current_type = plant;
		}
		else
		{
			GD.Print("Don't have that type yet!");
			return;
		}
		
		foreach (var model in plant_models.Values)
		{
			model.Visible = false;
		}
		
		GD.Print($"I am now {plant}!");

		plant_models[plant].Visible = true;
	}

	private void UnlockNewType(PlantType plant)
	{
			UnlockedTypes.Add(plant);
	}
}

public enum PlantType
{
	Normal,
	BerryBush,
	Vine,
	Cactus,
	Mushroom,
	Flower
}
