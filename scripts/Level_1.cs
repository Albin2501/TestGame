using Godot;
using System;

public partial class Level_1 : Node2D
{
   // Information of children
   private AudioStreamPlayer ThemeAudio;

   // Logic
   private Boolean ActiveMute = false;

   public override void _Ready()
   {
	  ThemeAudio = GetNode<AudioStreamPlayer>("Theme");
   }

   public override void _Process(double delta)
   {
	  HandleAudio();
	  HandleInput();
   }

   private void HandleAudio()
   {
	  if (ActiveMute)
	  {
		 if (ThemeAudio.Playing)
			ThemeAudio.Stop();
	  }
	  else
	  {
		 if (!ThemeAudio.Playing)
			ThemeAudio.Play();
	  }
   }

   private void HandleInput()
   {
	  if (Input.IsActionJustPressed("mute"))
		 ActiveMute = !ActiveMute;
   }
}
