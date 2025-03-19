using Godot;
using WildJam78.Scripts.Bullets;

[GlobalClass]
public partial class InfiniteBulletLifespanLogic : BulletLifespanLogic {
    
    public override bool IsDestroyed( float currentLifetime, float delta )
    {
        return false;
    }
}