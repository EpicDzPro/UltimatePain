using Godot;
using System;

public partial class GAME : Control
{
	[Export] private Menu Menu;
	[Export] private UI UI;
	[Export] private Label fps;
	[Export] private Label speed;
	[Export] private Label bulletCount;
	[Export] private Label pickName;
	[Export] private PlayerScript player;
	public override void _Ready()
	{
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
		fps.Text = Engine.GetFramesPerSecond().ToString();
		speed.Text = player.LinearVelocity.Round().Length().ToString();
		bulletCount.Text = player.bulletCount.ToString();
		pickName.Text = player.pickName;
		ProcessInput((float)delta);
	}


    private void ProcessInput(float delta)
    {
		
        player.Direction = Vector3I.Zero;

		player.Direction += player.MovementDirection.GlobalBasis.Z * Input.GetAxis("forward","back");
		player.Direction += player.MovementDirection.GlobalBasis.X * Input.GetAxis("left","right");

		player.Direction = player.Direction.Normalized();
		player.Direction = player.Direction.Slide(player.Normal);
		player.WishJump = Input.IsActionJustPressed("up");
		player.WishSprint = Input.GetActionStrength("sprint");
		player.WishDuck = Input.GetActionStrength("down");
		player.shoot = Input.IsActionPressed("fire");
		player.aim = Input.IsActionPressed("scope");
		if(Input.GetVector("back","forward","right","left").Length()>0)
		{
			player.Angle = Mathf.LerpAngle(player.Angle,Input.GetVector("back","forward","right","left").Angle(),delta*10);
		}
		player.xx = UI.xx;
		player.yy = UI.yy;
    }


    public override void _Input(InputEvent @event)
    {
		if(Input.IsActionJustPressed("ui_cancel"))
		{
			UI.Visible = !UI.Visible;
			Menu.Visible = !Menu.Visible;
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		if(Input.IsActionJustPressed("tps"))
		{
			player.tps = !player.tps;
		}
    }
}
