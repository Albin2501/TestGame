using System;
using System.Collections.Generic;
using Godot;

public partial class Player : CharacterBody2D
{
	// Physics
	private Vector2 CurrentVelocity = new(0, 0);
	private float Speed = 100;
	private float Gravity = 500;
	private float FallForce = 2000;
	private float JumpForce = -250;
	private float DashForce = 250;

	// Information of children
	private AnimationPlayer Animation;
	private List<Sprite2D> Sprites = [];
	private Camera2D Camera;
	private AudioStreamPlayer2D StepAudio;
	private AudioStreamPlayer2D JumpAudio;

	// Logic
	private Boolean ActiveJump = false;
	private Boolean DoubleJump = true;
	private float DashTime = 1f;
	private float DashCounter = 0f;
	private float CoyoteTime = 0.5f;
	private float CoyoteCounter = 0f;

	private int AppleCounter = 0;

	public override void _Ready()
	{
		Animation = GetNode<AnimationPlayer>("Animation");
		Sprites.Add(GetNode<Sprite2D>("IdleSprite"));
		Sprites.Add(GetNode<Sprite2D>("StepSprite"));
		Sprites.Add(GetNode<Sprite2D>("FallSprite"));
		Camera = GetNode<Camera2D>("Camera");
		StepAudio = GetNode<AudioStreamPlayer2D>("Step");
		JumpAudio = GetNode<AudioStreamPlayer2D>("Jump");

		Sprites.ForEach(Sprite => Sprite.Visible = false);

		Camera.MakeCurrent();
	}

	public override void _Process(double delta)
	{
		HandleInput((float)delta);
		HandleAudio();
		HandleAnimation();
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyGravity((float)delta);
		MoveCharacter((float)delta);
		HandleMechanics((float)delta);
	}

	public void OnApplePickUp()
	{
		AppleCounter += 1;
		GD.Print(AppleCounter);
	}

	// ================================================================
	// ======================= Helper Functions =======================
	// ================================================================

	private void ApplyGravity(float delta)
	{
		CurrentVelocity.Y += Gravity * delta;
		CurrentVelocity.Y = Math.Min(FallForce, CurrentVelocity.Y);
	}

	private void MoveCharacter(float delta)
	{
		Velocity = CurrentVelocity;
		MoveAndSlide();

		if (IsOnFloor() || IsOnCeiling())
			CurrentVelocity.Y = 0;
	}

	private void HandleMechanics(float delta)
	{
		if (IsOnFloor())
		{
			CoyoteCounter = CoyoteTime;
			DashCounter = DashTime;
			DoubleJump = true;
		}
		else
			CoyoteCounter = Math.Max(0, CoyoteCounter - delta);

		if (DashCounter < DashTime)
		{
			DashCounter = Math.Max(0, DashCounter - delta);
		}
	}

	private void HandleInput(float delta)
	{
		if (Input.IsActionPressed("right"))
			CurrentVelocity.X = Speed;
		else if (Input.IsActionPressed("left"))
			CurrentVelocity.X = -Speed;
		else
			CurrentVelocity.X = (Math.Abs(CurrentVelocity.X) < 0.1f) ? 0 : CurrentVelocity.X * 0.75f;

		if (Input.IsActionPressed("down"))
			CurrentVelocity.Y = Speed * FallForce;

		if (Input.IsActionJustPressed("dash") && !IsOnFloor() && DashCounter == DashTime)
		{
			// TODO: this doesn't work like I want it to
			//CurrentVelocity.X += Math.Sign(CurrentVelocity.X) * DashForce;
			Position = new(Math.Sign(CurrentVelocity.X) * DashForce, Position.Y);
			DashCounter -= delta;
		}

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

	private void HandleAnimation()
	{
		if (IsOnFloor() && Velocity.X == 0)
		{
			Animation.Play("idle");
			Sprites.ForEach(Sprite => Sprite.Visible = Sprite.Name == "IdleSprite");
		}
		else if (IsOnFloor() && Velocity.X != 0)
		{
			Animation.Play("step");
			Sprites.ForEach(Sprite => Sprite.Visible = Sprite.Name == "StepSprite");
		}
		else if (!IsOnFloor())
		{
			// TODO: this animation is still buggy
			Animation.Play("fall");
			Sprites.ForEach(Sprite => Sprite.Visible = Sprite.Name == "FallSprite");
		}

		if (Input.IsActionPressed("right"))
			Sprites.ForEach(Sprite => Sprite.FlipH = false);
		else if (Input.IsActionPressed("left"))
			Sprites.ForEach(Sprite => Sprite.FlipH = true);
	}
}
