using Godot;

namespace WildJam78.Scripts.Bullets;

[GlobalClass]
public partial class BulletDeathLogic : Resource {
    [Export] 
    public int Count = 10;
    [Export]
    public PackedScene BulletPrefab;

}