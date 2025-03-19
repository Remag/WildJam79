using Godot;
using WildJam78.Scripts.Bullets;

[GlobalClass]
public partial class LinearBulletMoveLogic : BulletMoveLogic {
    
    [Export]
    public float startMinVelocity = 300;
    [Export]
    public float startMaxVelocity = 300;
    [Export]
    public float maxVelocity = 300;
    [Export]
    public float acceleration = 0;
    
    public float GetStartVelocity() => Rng.RandomRange( startMinVelocity, startMaxVelocity );
}