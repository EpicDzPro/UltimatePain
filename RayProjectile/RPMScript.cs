using Godot;
using System;
using System.Collections.Generic;

public partial class RPMScript : Node3D
{
	public List<RayProjectile> Projectiles = new List<RayProjectile>();
	private List<RayProjectile> Delete = new List<RayProjectile>();
	public override void _Ready()
	{
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		foreach(RayProjectile projectile in Projectiles)
		{
			Vector3 position = projectile.transform.Origin;
			Vector3 direction = projectile.transform.Basis.Y;
			

			PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
			PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(position - direction/2,position + direction/2);
			query.HitBackFaces = false;
			query.CollideWithAreas = true;
			Godot.Collections.Dictionary result = spaceState.IntersectRay(query);


			if(result.Count != 0)
			{
				Vector3 normal = (Vector3)result["normal"];
				if(normal.Dot(-projectile.transform.Basis.Y)<0.25f)
				{
					projectile.transform.Basis.Y = projectile.transform.Basis.Y.Bounce(normal);
				}
				else
				{
					Delete.Add(projectile);
					continue;
				}
				
			}
			if(projectile.Time > 8)
			{
				Delete.Add(projectile);
				continue;
			}
			projectile.Speed += (float)delta*projectile.Accel*0.5f;
			projectile.transform.Origin += direction * projectile.Speed * (float)delta;
			projectile.Speed += (float)delta*projectile.Accel*0.5f;

			projectile.Time += (float)delta;

			RenderingServer.InstanceSetTransform(projectile.instance,projectile.transform);
		}

		foreach(RayProjectile projectile in Delete)
		{
			RenderingServer.FreeRid(projectile.instance);
			Projectiles.Remove(projectile);
		}
		Delete.Clear();

	}
}

