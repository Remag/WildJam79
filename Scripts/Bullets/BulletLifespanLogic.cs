using Godot;

namespace WildJam78.Scripts.Bullets;

[GlobalClass]
public abstract partial class BulletLifespanLogic : Resource {
    public abstract bool IsDestroyed( float currentLifetime, float delta );
}