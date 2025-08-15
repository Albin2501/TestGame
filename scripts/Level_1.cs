using Godot;
using System;

public partial class Level_1 : Node2D
{
   // Information of children
   private AudioStreamPlayer ThemeAudio;

   public override void _Ready()
   {
	  ThemeAudio = GetNode<AudioStreamPlayer>("Theme");
	  ThemeAudio.Play();
   }

   public override void _Process(double delta)
   {
	  if (!ThemeAudio.Playing)
		 ThemeAudio.Play();
   }
}
