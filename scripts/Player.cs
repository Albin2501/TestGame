using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

// TODO: Jumping from Ground Bug
// TODO: Dash Bug

public partial class Player : CharacterBody2D
{
	// Constants
	private const float _StepSpeed = 100;
	private const float _GravitySpeed = 500;
	private const float _FallSpeed = 2000;
	private const float _JumpSpeed = -250;
	private const float _DashSpeed = 500;
	private const float _DashTime = 0.25f;
	private const float _CoyoteTime = 0.5f;

	// Information of children
	private AnimationPlayer _AnimationPlayer;
	private Camera2D _Camera;
	private readonly Dictionary<string, AudioStreamPlayer2D> _Audio = new();
	private readonly Dictionary<string, Sprite2D> _Sprites = new();

	// Logic
	private Vector2 _Velocity = new(0, 0); // Calculated velocity to apply after everything
	private bool _IsOnFloor = false; // As to not call the function too many times
	private bool _ActivatedJump = false;
	private bool _EligibleDoubleJump = true;
	private bool _ActiveDash = false;
	private bool _ActivatedDash = false;
	private bool _PlayAnimationOnce = false;
	private float _DashTimeLeft = 0f;
	private float _CoyoteTimeLeft = 0f;

	// =========================================================================
	// =========================================================================
	// Functions to set Player up

	public override void _Ready()
	{
		Godot.Collections.Array<Node> Children = GetChildren();

		foreach (Node Child in Children)
		{
			if (Child is Camera2D)
				_Camera = (Camera2D)Child;
			else if (Child is AnimationPlayer)
				_AnimationPlayer = (AnimationPlayer)Child;
			else if (Child is Sprite2D)
				_Sprites[Child.Name] = (Sprite2D)Child;
			else if (Child is AudioStreamPlayer2D)
				_Audio[Child.Name] = (AudioStreamPlayer2D)Child;
		}

		_Camera.MakeCurrent();
	}

	public override void _PhysicsProcess(double delta)
	{
		_IsOnFloor = IsOnFloor();
		float DeltaF = (float)delta;

		_HandleGravity(DeltaF);
		_HandleMechanics(DeltaF);
		_HandlePlayer(DeltaF);
	}

	public override void _Process(double delta)
	{
		float DeltaF = (float)delta;

		_HandleMovement(DeltaF);
		_HandleVisual(DeltaF);
		_HandleAudio(DeltaF);
		_CleanUp(DeltaF);
	}

	// =========================================================================
	// =========================================================================
	// Functions for interacting with Player

	public void OnApplePickUp()
	{
		GD.Print("You got an apple!");
	}

	// =========================================================================
	// =========================================================================
	// Internal logic for Player

	private void _HandleGravity(float delta)
	{
		// Always apply Gravity except when Player is dashing or grounded
		if (_ActiveDash || _IsOnFloor)
			_Velocity.Y = 0;
		else
		{
			_Velocity.Y += _GravitySpeed * delta;
			_Velocity.Y = Math.Min(_FallSpeed, _Velocity.Y);
		}
	}

	private void _HandleMechanics(float delta)
	{
		// Reset all mechanics when Player touches floor
		if (_IsOnFloor)
		{
			_CoyoteTimeLeft = _CoyoteTime;
			_DashTimeLeft = _DashTime;
			_EligibleDoubleJump = true;
		}

		// Count down the moment Player isn't grounded
		if (!_IsOnFloor)
			_CoyoteTimeLeft = Math.Max(0, _CoyoteTimeLeft - delta);

		// Count down the moment Player has dashed
		if (_ActiveDash)
			_DashTimeLeft = Math.Max(0, _DashTimeLeft - delta);

	}

	private void _HandlePlayer(float delta)
	{
		// Move Player after all calculations have been done
		Velocity = _Velocity;
		MoveAndSlide();
	}

	private void _HandleMovement(float delta)
	{
		// When step movement
		if (Input.IsActionPressed("right") && !_ActiveDash)
			_Velocity.X = _StepSpeed;
		else if (Input.IsActionPressed("left") && !_ActiveDash)
			_Velocity.X = -_StepSpeed;
		else if (!_ActiveDash)
			_Velocity.X = 0;

		// When dashing
		if (Input.IsActionJustPressed("dash") && !_IsOnFloor && !_ActiveDash)
		{
			// Dash was activated and is not eligible anymore
			_Velocity.X = _DashSpeed * (_Sprites.Values.First().FlipH ? -1 : 1);
			_ActivatedDash = true;
			_ActiveDash = false;
		}

		// When (double)jumping
		if (Input.IsActionJustPressed("jump") && (_CoyoteTimeLeft > 0f || _EligibleDoubleJump))
		{
			if (_CoyoteTimeLeft > 0f)
				_CoyoteTimeLeft = 0f;
			else
				_EligibleDoubleJump = false;

			_ActivatedJump = true;
			_Velocity.Y = _JumpSpeed;
		}
	}

	private void _HandleVisual(float delta)
	{
		// Flip Sprite to the movement direction
		if (Input.IsActionPressed("right"))
			_FlipSpritesLeft(false);
		else if (Input.IsActionPressed("left"))
			_FlipSpritesLeft(true);

		if (_PlayAnimationOnce)
			return;

		// When standing still
		if (_IsOnFloor && Velocity.X == 0)
		{
			_AnimationPlayer.Play("IdleAnimation");
			_OnlyActiveSprite("IdleSprite");
			return;
		}

		// When walking while grounded 
		if (_IsOnFloor && Velocity.X != 0)
		{
			_AnimationPlayer.Play("StepAnimation");
			_OnlyActiveSprite("StepSprite");
			return;
		}

		// When falling
		if (!_IsOnFloor)
		{
			_AnimationPlayer.Play("FallAnimation");
			_OnlyActiveSprite("FallSprite");
			_PlayAnimationOnce = true;
			return;
		}
	}

	private void _HandleAudio(float delta)
	{
		// When running
		if (_IsOnFloor && Velocity.X != 0 && !_Audio["StepAudio"].Playing)
			_PlayAudio("StepAudio");

		// When jumping
		if (_ActivatedJump)
			_PlayAudio("JumpAudio");

		// Dash Audio
		if (_ActivatedDash)
			_PlayAudio("DashAudio");
	}

	private void _CleanUp(float delta)
	{
		if (_ActivatedJump)
			_ActivatedJump = false;

		if (_ActivatedDash)
			_ActivatedDash = false;

		if (_IsOnFloor)
			_PlayAnimationOnce = false;
	}

	// =========================================================================
	// =========================================================================
	// Helper functions

	private void _PlayAudio(string AudioName)
	{
		foreach (AudioStreamPlayer2D Audio in _Audio.Values)
		{
			if (Audio.Name == AudioName)
				Audio.Play();
		}
	}

	private void _FlipSpritesLeft(bool Flip)
	{
		foreach (Sprite2D Sprite in _Sprites.Values)
			Sprite.FlipH = Flip;
	}

	private void _OnlyActiveSprite(string SpriteName)
	{
		foreach (Sprite2D Sprite in _Sprites.Values)
			Sprite.Visible = Sprite.Name == SpriteName;
	}
}
