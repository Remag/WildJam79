using Godot;
using System;

public partial class FoodSource : RigidBody2D
{
    [Export]
    public int GeneralExp = 1;
    [Export]
    public int WeaponXp = 1;
    [Export]
    public PackedScene CorePrefab;
    [Export]
    public bool IsWeaponSource;
}
