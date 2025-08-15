using Godot;
using System;

public partial class Camera : Camera2D
{
	// Information of children
	private AudioStreamPlayer2D InAudio;
	private AudioStreamPlayer2D OutAudio;

	// Logic
   private int ZoomLevel = 0;
   private float ZoomFactor = 1.5f;

	public override void _Ready()
	{
		InAudio = GetNode<AudioStreamPlayer2D>("In");
		OutAudio = GetNode<AudioStreamPlayer2D>("Out");
	}

   public override void _Process(double delta)
   {
	  HandleInput((float)delta);
   }

   // ================================================================
   // ======================= Helper Functions =======================
   // ================================================================

   private void HandleInput(float delta)
   {
	  if (Input.IsActionJustPressed("in") && ZoomLevel < 2)
	  {
		 ZoomLevel++;
		 Zoom = new Vector2(Zoom.X * ZoomFactor, Zoom.Y * ZoomFactor);
		 InAudio.Play();
		}


	  if (Input.IsActionJustPressed("out") && ZoomLevel > 0)
	  {
		 ZoomLevel--;
		 Zoom = new Vector2(Zoom.X * 1 / ZoomFactor, Zoom.Y * 1 / ZoomFactor);
		 OutAudio.Play();
		}
   }
}
