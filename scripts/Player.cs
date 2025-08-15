using System;
using Godot;

public partial class Player : CharacterBody2D
{
	// Physics
	private Vector2 CurrentVelocity = new(0, 0);
	private float Speed = 100;
	private float Gravity = 200;
	private float FallSpeed = 2000;
	private float JumpForce = -150;

	// Information of children
	private AnimationPlayer animation;
	private Sprite2D sprite;
	private Camera2D camera;

	// Logic
	private Boolean DoubleJump = true;
	private float CoyoteTime = 0.1f;
	private float CoyoteCounter = 0f;

	public override void _Ready()
	{
		animation = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<Sprite2D>("Sprite2D");
		camera = GetNode<Camera2D>("Camera2D");

		animation.Play("idle");
		camera.MakeCurrent();
	}

	public override void _Process(double delta)
	{
		HandleInput((float)delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyGravity((float)delta);
		MoveCharacter((float)delta);
		HandleMechanics();
	}

	// ================================================================
	// ======================= Helper Functions =======================
	// ================================================================

	private void ApplyGravity(float delta)
	{
		CurrentVelocity.Y += Gravity * delta;
		CurrentVelocity.Y = Math.Min(FallSpeed, CurrentVelocity.Y);
	}

	private void MoveCharacter(float delta)
	{
		Velocity = CurrentVelocity;
		MoveAndSlide();

		if (IsOnFloor())
			CurrentVelocity.Y = 0;
	}

	private void HandleMechanics()
	{
		if (IsOnFloor())
		{
			CoyoteCounter = CoyoteTime;
			DoubleJump = true;
		}
	}

	private void HandleInput(float delta)
	{
		if (Input.IsActionPressed("right"))
		{
			sprite.FlipH = false;
			CurrentVelocity.X = Speed;
		}
		else if (Input.IsActionPressed("left"))
		{
			sprite.FlipH = true;
			CurrentVelocity.X = -Speed;
		}
		else
			CurrentVelocity.X = 0;


		if (Input.IsActionJustPressed("jump") && (CoyoteCounter > 0f || DoubleJump))
		{
			if (CoyoteCounter > 0f)
				CoyoteCounter = 0f;
			else if (DoubleJump)
				DoubleJump = false;

			CurrentVelocity.Y = JumpForce;
		}

		// Reset to middle of the screen
		if (Input.IsKeyPressed(Key.R))
		{
			Velocity = new(0, 0);
			Position = new(0, 0);
		}
	}
}
