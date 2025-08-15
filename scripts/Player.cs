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
	private AnimationPlayer IdleAnimation;
	private Sprite2D Sprite;
	private Camera2D Camera;
	private AudioStreamPlayer2D StepAudio;
	private AudioStreamPlayer2D JumpAudio;

	// Logic
	private Boolean ActiveJump = false;
	private Boolean DoubleJump = true;
	private float CoyoteTime = 0.1f;
	private float CoyoteCounter = 0f;

	public override void _Ready()
	{
		IdleAnimation = GetNode<AnimationPlayer>("Animation");
		Sprite = GetNode<Sprite2D>("Sprite");
		Camera = GetNode<Camera2D>("Camera");
		StepAudio = GetNode<AudioStreamPlayer2D>("Step");
		JumpAudio = GetNode<AudioStreamPlayer2D>("Jump");

		IdleAnimation.Play("idle");
		Camera.MakeCurrent();
	}

	public override void _Process(double delta)
	{
		HandleInput((float)delta);
		HandleAudio();
	}

	private void HandleAudio()
	{
		// Running Audio
		if (Velocity.X != 0 && IsOnFloor())
		{
			if (!StepAudio.Playing)
				StepAudio.Play();
		}
		else
			StepAudio.Stop();

		// Jumping Audio
		if (ActiveJump)
		{
			JumpAudio.Play();
			ActiveJump = false;
		}
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
			Sprite.FlipH = false;
			CurrentVelocity.X = Speed;
		}
		else if (Input.IsActionPressed("left"))
		{
			Sprite.FlipH = true;
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
			
			ActiveJump = true;
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
