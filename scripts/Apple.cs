using Godot;
using System.Collections.Generic;

public partial class Apple : Area2D
{
   // Information of children
   private AnimationPlayer Animation;
   private List<Sprite2D> Sprites = [];
   private AudioStreamPlayer2D PickUpAudio;


   public override void _Ready()
   {
	  Animation = GetNode<AnimationPlayer>("Animation");
	  Sprites.Add(GetNode<Sprite2D>("Sprite"));
	  Sprites.Add(GetNode<Sprite2D>("PickUpSprite"));
	  PickUpAudio = GetNode<AudioStreamPlayer2D>("PickUp");

	  Sprites.ForEach(Sprite => Sprite.Visible = Sprite.Name == "Sprite");
	  BodyEntered += OnApplePickUp;
   }

	private void OnApplePickUp(Node body)
   {
	  if (body is Player player)
	  {
	  player.OnApplePickUp();
	  Sprites.ForEach(Sprite => Sprite.Visible = Sprite.Name == "PickUpSprite");
	  Animation.Play("pick_up");
	  PickUpAudio.Play();

	  Animation.AnimationFinished += animName => QueueFree();
	  }
   }

}
