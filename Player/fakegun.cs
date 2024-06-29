using Godot;
using System;

public partial class fakegun : RigidBody3D
{
	[Export] private Mesh[] blasters;
    [Export] CapsuleMesh projectileBase;
    bool allowed = true;
	public int bulletCount = 30;
	public bool picked = false;
	public Vector3 v = new  Vector3(0,0,0);
    public Vector3 f = new  Vector3(0,0,-1);
    public Vector3 u = new  Vector3(0,1,0);
	public override void _Ready()
	{
	}
    
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
        if(picked)
        {
            //LinearVelocity = v;
            LookFollow(state,f,u);
        }
	}
    private void LookFollow(PhysicsDirectBodyState3D state,Vector3 targetDirf,Vector3 targetDiru)
    {

        Vector3 forwardDir = -GlobalBasis.Z;
        Vector3 crossf = forwardDir.Cross(targetDirf).Normalized();
        float anglef = forwardDir.AngleTo(targetDirf);

        Vector3 upDir = GlobalBasis.Y;
        Vector3 crossu = upDir.Cross(targetDiru).Normalized();
        float angleu = upDir.AngleTo(targetDiru);


        if (crossf.IsNormalized() || crossf.IsNormalized())
        {
            state.AngularVelocity = (anglef * crossf + angleu * crossu)*32;
        }
        

    }

	public async void fire(RPMScript manager)
	{
        if(!allowed || bulletCount < 0)return;
		allowed = false;
		bulletCount -= 1;
        RayProjectile projectile = new RayProjectile
        {
            transform = GetNode<MeshInstance3D>("gunnosel").GlobalTransform,

            instance = RenderingServer.InstanceCreate(),
        };
        RenderingServer.InstanceSetTransform(projectile.instance,projectile.transform);
		RenderingServer.InstanceSetBase(projectile.instance,projectileBase.GetRid());
		RenderingServer.InstanceSetScenario(projectile.instance,GetWorld3D().Scenario);
		
		manager.Projectiles.Add(projectile);

        LinearVelocity = LinearVelocity + GlobalBasis.Z * 128;
		await ToSignal(GetTree().CreateTimer(0.1f),"timeout");
		allowed = true;

	}

	public async void reload(int b)
	{
        if(bulletCount >= 30)return;
		allowed = false;
		await ToSignal(GetTree().CreateTimer(1f),"timeout");
		bulletCount += 30 - bulletCount;
		allowed = true;
	}

}
