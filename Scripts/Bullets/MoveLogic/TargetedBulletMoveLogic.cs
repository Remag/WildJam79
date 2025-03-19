using Godot;
using WildJam78.Scripts.Bullets;

[GlobalClass]
public partial class TargetedBulletMoveLogic : BulletMoveLogic {
    
    [Export]
    public float startMinVelocity = 300;
    [Export]
    public float startMaxVelocity = 300;
    [Export]
    public float maxVelocity = 300;
    [Export]
    public float angularVelocity = float.Pi / 8;
    [Export]
    public float acceleration = 0;
    [Export]
    public float activeTime = 6;
    [Export]
    public float inactiveFriction = 300;
    
    public float GetStartVelocity() => Rng.RandomRange( startMinVelocity, startMaxVelocity );
}