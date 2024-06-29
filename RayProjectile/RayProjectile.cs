using Godot;
using System;

public partial class RayProjectile
{
    public Transform3D transform;
    public float Time = 0;
    public float Speed = 100;
    public float Accel = 100;
    public float Damage = 10;
    public String Owner = "";
    public String Type = "";

    public Rid instance;
}
