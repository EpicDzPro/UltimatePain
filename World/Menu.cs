using Godot;
using System;

public partial class Menu : Control
{
    public override void _GuiInput(InputEvent @event)
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
}
