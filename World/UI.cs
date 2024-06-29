using Godot;
using System;

public partial class UI : Control
{
    public float xx;
    public float yy;
    public override void _GuiInput(InputEvent @event)
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        if(@event is InputEventMouseMotion mouseMotion)
		{
			xx -= mouseMotion.Relative.Y * 0.5f * (float)GetPhysicsProcessDeltaTime();
			yy -= mouseMotion.Relative.X * 0.5f * (float)GetPhysicsProcessDeltaTime();
			xx = Mathf.Clamp(xx,-Mathf.Pi/2,Mathf.Pi/2);
		}
    }
}

