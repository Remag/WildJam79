using Godot;

namespace WildJam78.Scripts.Bullets;

[GlobalClass]
public abstract partial class BulletLifespanLogic : Resource {
    [Export] 
    public float AlphaTweenTime = 0.5f;

    public abstract bool IsDestroyed( float currentLifetime, float delta );
}