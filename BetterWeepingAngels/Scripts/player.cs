using Godot;
using System;

public partial class player : CharacterBody3D
{
	public const float Speed = 3.0f;
	public const float JumpVelocity = 4.5f;
	public const float Sensitivity = 5.00f / 100f;

	public AudioStreamPlayer3D aux;

	public Camera3D cam;

	public override void _Ready()
	{
		base._Ready();

		cam = GetChild<Camera3D>(2);
		aux = GetNode<AudioStreamPlayer3D>("aux");
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventKey) {
			RotateY(-eventKey.Relative.X * Sensitivity);
			cam.RotateX(-eventKey.Relative.Y * Sensitivity);
			cam.Rotation = new Vector3((float) Math.Clamp(cam.Rotation.X, -1.1, 1.1), 0, 0);
		}
	}


	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{

		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{

			aux.PitchScale = new RandomNumberGenerator().RandfRange(1.5f, 2f);
			if (aux.StreamPaused)
			{
				aux.Play();
			}
			aux.StreamPaused = false;

			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			aux.StreamPaused = true;
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
