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
	private float DashForce = 500;

	// Information of children
	private AnimationPlayer Animation;
	private List<Sprite2D> Sprites = [];
	private Camera2D Camera;
	private AudioStreamPlayer2D StepAudio;
	private AudioStreamPlayer2D JumpAudio;
	private AudioStreamPlayer2D DashAudio;

	// Logic
	private Boolean ActiveJump = false;
	private Boolean DoubleJump = true;
	private Boolean ActivatedDash = false;
	private float DashTime = 0.25f;
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
		DashAudio = GetNode<AudioStreamPlayer2D>("Dash");

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
		if (DashCounter <= 0 || DashCounter == DashTime)
		{
			CurrentVelocity.Y += Gravity * delta;
			CurrentVelocity.Y = Math.Min(FallForce, CurrentVelocity.Y);
		}
		else
			CurrentVelocity.Y = 0;
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
			DashCounter -= delta;
		}


	}

	private void HandleInput(float delta)
	{
		if (Input.IsActionPressed("right") && (DashCounter <= 0 || DashCounter == DashTime))
			CurrentVelocity.X = Speed;
		else if (Input.IsActionPressed("left") && (DashCounter <= 0 || DashCounter == DashTime))
			CurrentVelocity.X = -Speed;
		else if (DashCounter <= 0 || DashCounter == DashTime)
			CurrentVelocity.X = 0;

		if (Input.IsActionPressed("down"))
			CurrentVelocity.Y = Speed * FallForce;

		if (Input.IsActionJustPressed("dash") && !IsOnFloor() && DashCounter == DashTime)
		{
			CurrentVelocity.X = DashForce * (Sprites[0].FlipH ? -1 : 1);
			ActivatedDash = true;
			DashCounter -= delta;
		}

		if (Input.IsActionJustPressed("jump") && (CoyoteCounter > 0f || DoubleJump)
			&& (DashCounter <= 0 || DashCounter == DashTime))
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

		// Dash Audio
		if (ActivatedDash)
		{

			DashAudio.Play();
			ActivatedDash = false;
		}
	}

	private Boolean StopAnimation = false;

	private void HandleAnimation()
	{
		if (Input.IsActionPressed("right"))
			Sprites.ForEach(Sprite => Sprite.FlipH = false);
		else if (Input.IsActionPressed("left"))
			Sprites.ForEach(Sprite => Sprite.FlipH = true);

		if (IsOnFloor())
			StopAnimation = false;

		if (StopAnimation)
				return;

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
			Animation.Play("fall");
			StopAnimation = true;
			Sprites.ForEach(Sprite => Sprite.Visible = Sprite.Name == "FallSprite");
		}
	}
}
