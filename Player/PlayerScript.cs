using Godot;
using System;
using System.Data;
using System.Security.Cryptography.X509Certificates;

public partial class PlayerScript : RigidBody3D
{
	[Export] AnimationPlayer APlayer;
	[Export] Camera3D Camera;
	[Export] RayCast3D CameraRay;
	[Export] SkeletonIK3D right;
	[Export] SkeletonIK3D left;
	[Export] RayCast3D pickRay;
	[Export] Marker3D itemPickMarker;
	[Export] Marker3D gunPickMarker;
	[Export] Marker3D meleePickMarker;
	
	private RigidBody3D pickedItem = null;
	private fakegun pickedGun = null;
	private MeleeWeapon pickedMelee = null;
	public String pickName = "";
	[Export] RPMScript maneger;
	private float MaxVeAir = 10f;
	private static float MaxVeGround = 12.0f;
	private float MaxA = 100f * MaxVeGround;
	private float StopSpeed = 8f;
	private static float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	public float JumpImpulse = 64f * Gravity;
	private float gforce = 0;
	public Vector3 Normal = Vector3.Zero;
	public Vector3 Direction;
	public bool WishJump = false;
	public bool Jump = false;
	public float WishSprint = 1f;
	public float WishDuck = 1f;
	public bool OnFloor = false;
	public float Angle = 0;
	public bool tps = false;
	public bool aim = false;
	public bool shoot = false;

	public Node3D MovementDirection;
	public Node3D CameraHolder;
	public Node3D WeaponHolder;
	
	public float xx = 0f;
	public float yy = 0f;
	public int bulletCount = 210;


    public override void _Ready()
    {
		MovementDirection = GetNode<Node3D>("MovementDirection");
		CameraHolder = GetNode<Node3D>("CameraHolder");
		WeaponHolder = GetNode<Node3D>("WeaponHolder");
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }
	////////////////////////Input////////////////////////
    public override void _Input(InputEvent @event)
    {
		if(Input.IsActionJustPressed("interact"))
		{
			if(pickedItem == null && pickedGun == null && pickedMelee == null)
			{
				if(pickRay.IsColliding())
				{
					switch(pickRay.GetCollider())
					{
						case MeleeWeapon:
							pickedMelee = (MeleeWeapon)pickRay.GetCollider();
							pickedMelee.GlobalTransform = meleePickMarker.GlobalTransform;
							pickName = pickedMelee.Name;
							pickedMelee.picked = true;
							left.TargetNode = pickedMelee.GetNode<Marker3D>("left").GetPath();
							right.TargetNode = pickedMelee.GetNode<Marker3D>("right").GetPath();
							break;
						case fakegun:
							pickedGun = (fakegun)pickRay.GetCollider();
							pickedGun.GlobalTransform = gunPickMarker.GlobalTransform;
							pickName = pickedGun.Name;
							pickedGun.picked = true;
							left.TargetNode = pickedGun.GetNode<Marker3D>("left").GetPath();
							right.TargetNode = pickedGun.GetNode<Marker3D>("right").GetPath();
							break;
						case RigidBody3D:
							pickedItem = (RigidBody3D)pickRay.GetCollider();
							pickName = pickedItem.Name;
							break;
				}	}

			}
			else
			{
				if(pickedGun != null)
				{
					pickedGun.picked = false;
					left.TargetNode = GetNode<Node3D>("WeaponHolder/left").GetPath();
					right.TargetNode = GetNode<Node3D>("WeaponHolder/right").GetPath();
					pickedGun = null;
				}
				if(pickedMelee != null)
				{
					pickedMelee.picked = false;
					left.TargetNode = GetNode<Node3D>("WeaponHolder/left").GetPath();
					right.TargetNode = GetNode<Node3D>("WeaponHolder/right").GetPath();
					pickedMelee = null;
				}

				if(pickedItem != null)pickedItem = null;
				
				pickName = "";
			}

		}
		if(Input.IsActionJustPressed("reload") && bulletCount > 0 && pickedGun != null)
		{
			bulletCount -= 30-pickedGun.bulletCount;
			pickedGun.reload(bulletCount);
		}
		if(Input.IsActionJustPressed("down"))
		{
			GetNode<CollisionShape3D>("duck").Disabled = !GetNode<CollisionShape3D>("duck").Disabled;
			GetNode<CollisionShape3D>("stand").Disabled = !GetNode<CollisionShape3D>("stand").Disabled;
		}


    }
	////////////////////////Physics_Proceces////////////////////////
	public void FirstPersonShooter(float delta)
	{
		float blend = 1 - Mathf.Pow(0.5f,delta*64);
		MovementDirection.Quaternion = new Quaternion(Vector3.Up,yy);
		CameraHolder.Quaternion = new Quaternion(Vector3.Up,yy)* new Quaternion(Vector3.Right,xx);
		WeaponHolder.Quaternion = CameraHolder.Quaternion;// WeaponHolder.Quaternion.Slerp(CameraHolder.Quaternion,blend);
		Camera.Position = Camera.Position.Lerp(Vector3.Zero,blend);

		gunPickMarker.Position = gunPickMarker.Position.MoveToward(new Vector3(0.25f,-0.3f,-1.25f),blend);
		if(aim)
		{
			gunPickMarker.Position = gunPickMarker.Position.MoveToward(new Vector3(0.0f,-0.2f,-1f),blend);
		}
		if(shoot && pickedGun != null)
		{
			pickedGun.fire(maneger);
		}

		if(shoot &&pickedItem != null)
		{
			if(WeaponHolder.GlobalBasis.Z.Dot(CameraHolder.GlobalBasis.Z)>0.9)
			{
				pickedItem.ApplyImpulse(CameraHolder.GlobalBasis.Z*-16);
				pickedItem = null;
				pickName = "";
			}
		}
		if(shoot && pickedMelee != null)
		{
			//pickedMelee.fire(meleePickMarker);
			APlayer.Play("guts");
		}

	}
	public void ThirdPersonShooter(float delta)
	{
		float blend = 1 - Mathf.Pow(0.5f,delta*64);
		MovementDirection.Quaternion = new Quaternion(Vector3.Up,yy);
		CameraHolder.Quaternion = new Quaternion(Vector3.Up,yy)* new Quaternion(Vector3.Right,xx);
		Camera.Position = Camera.Position.Lerp(new Vector3(1,0,2),blend);
		
		if(CameraRay.IsColliding() && false)
		{
			Vector3 targetDir = CameraRay.GetCollisionPoint().DirectionTo(Camera.GlobalPosition);
  	 	    Vector3 forwardDir = -Camera.GlobalBasis.Z;
    	    float anglef = forwardDir.AngleTo(targetDir);
			Camera.Quaternion = new Quaternion(Vector3.Up,anglef);
		}

		gunPickMarker.Position = gunPickMarker.Position.MoveToward(new Vector3(0.0f,-0.3f,-1.25f),delta*2);
		if(aim || shoot)
		{
			//gunPickMarker.Position = gunPickMarker.Position.MoveToward(new Vector3(0,-0.2f,-0.75f),delta*4);
			WeaponHolder.Quaternion = WeaponHolder.Quaternion.Slerp(CameraHolder.Quaternion,blend);
		}
		else if(Direction.Length()>0)
		{
			Quaternion whQ = new Quaternion(Vector3.Up,Angle+yy);
			WeaponHolder.Quaternion = WeaponHolder.Quaternion.Slerp(whQ,blend);
		}
		
		if(shoot && pickedGun != null)
		{
			if(pickedGun.GlobalBasis.Z.Dot(CameraHolder.GlobalBasis.Z)>0.9)
			{
				pickedGun.fire(maneger);
			}
			
		}

		if(shoot && pickedItem != null)
		{
			if(WeaponHolder.GlobalBasis.Z.Dot(CameraHolder.GlobalBasis.Z)>0.9)
			{
				pickedItem.ApplyImpulse(CameraHolder.GlobalBasis.Z*-16);
				pickedItem = null;
				pickName = "";
			}
		}

		if(shoot && pickedMelee != null)
		{
			//pickedMelee.fire(meleePickMarker);
			APlayer.Play("guts");
		}

	}
	////////////////////////Forcecee////////////////////////
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
		OnFloor = IsOnFloor(state);
		switch(tps)
		{
			case true:
				ThirdPersonShooter(state.Step);
				break;
			case false:
				FirstPersonShooter(state.Step);
				break;
		}
		
		Camera.Fov = Mathf.Lerp(Camera.Fov,(WishSprint * 0.25f + 1)*90,state.Step*10);


        ProcessMovement(state);
		if(pickedItem != null)
		{
			Vector3 a = itemPickMarker.GlobalPosition;
			Vector3 b = pickedItem.GlobalPosition;
			pickedItem.LinearVelocity = (a-b)*32 + LinearVelocity;

		}
		if(pickedGun != null)
		{
			Vector3 a = gunPickMarker.GlobalPosition;
			Vector3 b = pickedGun.GlobalPosition;
			pickedGun.LinearVelocity = (a-b)*32 + LinearVelocity;
			pickedGun.f = -WeaponHolder.GlobalBasis.Z;
			pickedGun.u = WeaponHolder.GlobalBasis.Y;
			if(CameraRay.IsColliding()&&shoot)
			{
				pickedGun.f = b.DirectionTo(CameraRay.GetCollisionPoint());
			}
		}
		if(pickedMelee != null)
		{
			Vector3 a = meleePickMarker.GlobalPosition;
			Vector3 b = pickedMelee.GlobalPosition;
			pickedMelee.LinearVelocity = (a-b)*64 + LinearVelocity;
			//pickedMelee.f = -meleePickMarker.GlobalBasis.Z;
			//pickedMelee.u = meleePickMarker.GlobalBasis.Y;
			pickedMelee.LookFollow(-meleePickMarker.GlobalBasis.Z,meleePickMarker.GlobalBasis.Y);
			//APlayer.Play("");
		}

    }

    private void ProcessMovement(PhysicsDirectBodyState3D state)
    {
		GetNode<CollisionShape3D>("duck").Disabled = WishDuck == 0;
		GetNode<CollisionShape3D>("stand").Disabled = WishDuck != 0;

		//Direction = Direction.Slide(Normal);
		switch (OnFloor)
		{
			case true:
				if(WishJump)
				{
					gforce = -Gravity*32;
					AirMovement(state.Step);
					Jump = true;
				}
				else
				{
					gforce = 0;
					GroundMovement(state.Step);
				}
				break;
			case false:
				if(Jump)gforce += 0.5f;
				else gforce += 1f;
				//gforce = Mathf.Clamp(gforce, -Gravity,Gravity);
				AirMovement(state.Step);
				break;
				
		}
		
    }
	private void GroundMovement(float delta)
	{
		float blend = 1 - Mathf.Pow(0.5f,delta*32);
		float Speed = LinearVelocity.Length();
		if(Speed != 0)
		{
			float Control = Mathf.Max(StopSpeed,Speed) * delta;
			LinearVelocity *= Mathf.Max(Speed - Control, 0) / Speed;
		}
		float CurrentSpeed = LinearVelocity.Dot(Direction);
		float AddSpeed = Mathf.Clamp(MaxVeGround * (WishSprint * 0.5f + 1)  - CurrentSpeed,0,MaxA * delta);
		LinearVelocity = LinearVelocity.Lerp(Direction * 8 - MovementDirection.GlobalBasis.Y * gforce,blend);
		
	}
	private void AirMovement(float delta)
	{
		float blend = 1 - Mathf.Pow(0.5f,delta*32);
		float CurrentSpeed = LinearVelocity.Dot(Direction);
		float AddSpeed = Mathf.Clamp(MaxVeGround * (WishSprint * 0.5f + 1) - CurrentSpeed,0,MaxA * delta);
		LinearVelocity = LinearVelocity.Lerp(Direction * 8 - MovementDirection.GlobalBasis.Y * gforce,blend);
	}
    public bool IsOnFloor(PhysicsDirectBodyState3D state)
	{
		
		for(int i = 0;i<state.GetContactCount();i++)
		{
			Normal = state.GetContactLocalNormal(i);
			if(Normal.Dot(Vector3.Up)> 0.5)
			{
				
				return true;
			}
		}
		return false;
	}




}
