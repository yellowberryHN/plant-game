using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	
	[Export]
	public PlantType current_type = PlantType.Normal;
	
	private List<PlantType> unlocked_types = new() { PlantType.Normal };
	private Dictionary<PlantType, Node3D> plant_models = new();
	private int type_index = 0;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

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
		
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Ready()
	{
		foreach (var plant in GetNode("Pot/Plant").GetChildren())
		{
			plant_models.Add(Enum.Parse<PlantType>(plant.Name), GetNode<Node3D>($"Pot/Plant/{plant.Name}"));
		}
		ShowPlant(current_type);
		
		// for debugging
		UnlockType(PlantType.BerryBush);

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

	private void CycleType()
	{
		type_index = (type_index + 1) % unlocked_types.Count;
		current_type = unlocked_types[type_index];
		ShowPlant(current_type);
		GD.Print($"I am now {current_type}!");
	}

	private void ShowPlant(PlantType plant)
	{
		foreach (var model in plant_models.Values)
		{
			model.Visible = false;
		}

		plant_models[plant].Visible = true;
	}

	private void UnlockType(PlantType plant)
	{
		if (!unlocked_types.Contains(plant))
		{
			unlocked_types.Add(plant);
		}
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