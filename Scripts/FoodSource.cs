using Godot;
using System;

public abstract partial class FoodSource : RigidBody2D {
    [Export]
    public int GeneralExp = 1;
    [Export]
    public int WeaponXp = 1;
    [Export]
    public PackedScene CorePrefab;
    [Export]
    public bool IsWeaponSource;

    public abstract Node2D GetTentacleAnchor();
    public abstract void OnTentacleCollision();
    public abstract bool TryTentaclePull( Tentacle tentacle );
    public abstract void OnBroughtToPlayer();
}
