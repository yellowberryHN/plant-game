using Godot;
using System.Collections.Generic;
using Godot.Collections;

public partial class Player : CharacterBody3D
{	
	public float Speed;
	public float JumpVelocity;
	
	private float turnSpeed = 12f;
	
	[Export]
	public PlantType current_type = PlantType.Normal;

	[Export] public Node3D CameraTarget;
	[Export] public Node3D CameraParent;

	private float cameraTilt;
	private float cameraSpeed;
	
	private Node3D pot;
	
	[Export] public Array<Texture2D> Faces = new()
	{
		GD.Load<Texture2D>("res://resources/textures/faces/Happy.png"),
		GD.Load<Texture2D>("res://resources/textures/faces/Sad.png"),
		GD.Load<Texture2D>("res://resources/textures/faces/VerySad.png"),
		GD.Load<Texture2D>("res://resources/textures/faces/Smile.png"),
		GD.Load<Texture2D>("res://resources/textures/faces/Shock.png"),
		GD.Load<Texture2D>("res://resources/textures/faces/Shoot.png"),
		GD.Load<Texture2D>("res://resources/textures/faces/Pleased.png"),
		GD.Load<Texture2D>("res://resources/textures/faces/Unsure.png"),
		GD.Load<Texture2D>("res://resources/textures/faces/Upset.png")
	};
	
	private Decal face;

	public List<PlantType> UnlockedTypes = new() { PlantType.Normal };

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	
	private System.Collections.Generic.Dictionary<PlantType, Node3D> plant_models = new();
	private int type_index = 0;

	
	
	private const float DEFAULT_SPEED = 15.0f;
	private const float DEFAULT_JUMP_VELOCITY = 11.0f;
	
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("respawn")) Respawn();

		CameraSmoothFollow(delta);
		Vector3 velocity = Velocity;
		Vector3 rotation = Rotation;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle floor logic
		if (IsOnFloor())
		{
			if (Input.IsActionJustPressed("swap_up"))
				CycleType(1);
			if (Input.IsActionJustPressed("swap_down"))
				CycleType(-1);
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
		
		pot = GetNode<Node3D>("Pot");
		face = pot.GetNode<Decal>("Face");
		
		foreach (var plant in pot.GetNode("Plant").GetChildren())
		{
			plant_models.Add(System.Enum.Parse<PlantType>(plant.Name), pot.GetNode<Node3D>($"Plant/{plant.Name}"));
		}
		SetPlant(current_type);
		
		// for debugging
		// if (OS.HasFeature("editor"))
		// {
		// 	UnlockNewType(PlantType.Flower);
		// 	UnlockNewType(PlantType.Mushroom);
		// }

		base._Ready();
	}

	private void SetFace(int idx)
	{
		if (idx >= Faces.Count) idx = 0;
		face.TextureAlbedo = Faces[idx];
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
		cameraSpeed = Speed * 16;
		float cameraTimer = Mathf.Clamp((float)delta * cameraSpeed / 20, 0, 1);
		Transform3D newTransform = CameraParent.GlobalTransform;
		newTransform.Origin = newTransform.Origin.Lerp(this.GlobalTransform.Origin + cameraOffset, cameraTimer);
		CameraParent.GlobalTransform = newTransform;
	}

	private void CycleType(int dir)
	{
		if (UnlockedTypes.Count < 2) return;
		type_index = (type_index + dir + UnlockedTypes.Count) % UnlockedTypes.Count;
		GD.Print($"type idx is {type_index}");
		PlantType newType = UnlockedTypes[type_index];
		SetPlant(newType);
	}

	private void SetPlant(PlantType plant)
	{
		SetFace((int)plant);
		if (UnlockedTypes.Contains(plant))
		{
			current_type = plant;
		}
		else
		{
			GD.Print("Don't have that type yet!");
			return;
		}

		switch (plant)
		{
			case PlantType.Mushroom:
				Speed = DEFAULT_SPEED * .60f;
				JumpVelocity = DEFAULT_JUMP_VELOCITY * 3;
				break;
			case PlantType.Flower:
				Speed = DEFAULT_SPEED * 3;
				JumpVelocity = 0f;
				break;
			default:
			case PlantType.Normal:
				Speed = DEFAULT_SPEED;
				JumpVelocity = DEFAULT_JUMP_VELOCITY;
				break;
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
		if (!UnlockedTypes.Contains(plant))
		{
			UnlockedTypes.Add(plant);
			
			// display message of "New plant unlocked!"
			Label unlockLabel = GetNode<Label>("../Control/UnlockLabel");
			unlockLabel.Text = $"New plant type unlocked: {plant}";
			unlockLabel.Show();
			
			Timer timer = unlockLabel.GetNode<Timer>("Timer");
			timer.Start();
		}
	}

	private void OnTimerTimeout()
	{
		GD.Print("Hiding label");
		Label unlockLabel = GetNode<Label>("../Control/UnlockLabel");
		unlockLabel.Hide();
	}

	private void Respawn()
	{
		GetTree().ReloadCurrentScene();
	}

	private void OnAreaEntered(Area3D area)
	{
		if (area is UnlockTrigger trigger)
		{
			UnlockNewType(trigger.unlockable);
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
