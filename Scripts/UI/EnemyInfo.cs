using Godot;
using System;

[GlobalClass]
public partial class EnemyInfo : Resource
{
    [Export]
    public PackedScene Prefab { get; set; }
    [Export]
    public int Count { get; set; } = 1;
}
