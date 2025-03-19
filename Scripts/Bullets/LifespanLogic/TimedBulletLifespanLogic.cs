using Godot;
using WildJam78.Scripts.Bullets;

[GlobalClass]
public partial class TimedBulletLifespanLogic : BulletLifespanLogic {
    
    [Export]
    private float _maxLifeTime = 10;
    public override bool IsDestroyed( float currentLifetime, float delta )
    {
        return currentLifetime >= _maxLifeTime;
    }
}