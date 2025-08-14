using Godot;
using System;

public partial class player : CharacterBody2D {
	private AnimationPlayer animPlayer;

	public override void _Ready()
	{
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animPlayer.Play("idle");
	}
}
