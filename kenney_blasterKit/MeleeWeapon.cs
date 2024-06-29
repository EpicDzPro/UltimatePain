using Godot;
using System;

public partial class MeleeWeapon : RigidBody3D
{
	[Export] private Mesh[] Weapons;
    bool allowed = true;
	public bool picked = false;
    public Vector3 f = new  Vector3(0,0,-1);
    public Vector3 u = new  Vector3(0,1,0);
	public override void _Ready()
	{
	}
    public void LookFollow(Vector3 targetDirf,Vector3 targetDiru)
    {

        Vector3 forwardDir = -GlobalBasis.Z;
        Vector3 crossf = forwardDir.Cross(targetDirf).Normalized();
        float anglef = forwardDir.AngleTo(targetDirf);

        Vector3 upDir = GlobalBasis.Y;
        Vector3 crossu = upDir.Cross(targetDiru).Normalized();
        float angleu = upDir.AngleTo(targetDiru);


        if (crossf.IsNormalized() && crossf.IsNormalized())
        {
            AngularVelocity = (anglef * crossf + angleu * crossu)* 16;
        }
    }

	public async void fire(Marker3D meleePickMarker)
	{
        if(!allowed)return;
		allowed = false;
		Tween tw = GetTree().CreateTween();
		tw.TweenProperty(meleePickMarker,"rotation",new Vector3(45,0,0),1).AsRelative();
		//tw.TweenProperty(meleePickMarker,"rotation",new Vector3(-60,0,0),0.1);
        //tw.TweenProperty(meleePickMarker,"rotation",new Vector3(-45,0,0),0.4);
		await ToSignal(GetTree().CreateTimer(1f),"timeout");
		allowed = true;

	}


}
