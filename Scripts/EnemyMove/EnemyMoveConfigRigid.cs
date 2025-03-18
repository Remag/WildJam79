using Godot;

namespace WildJam78.Scripts.EnemyMove;

[GlobalClass]
public partial class EnemyMoveConfigRigid : Resource {
    [Export]
    public float minRadius = 100;
    [Export]
    public float maxRadius = 200;
    [Export]
    public float rngAngle = float.Pi / 2;
    [Export]
    public float newTargetChooseTime = 5;
    [Export]
    public float targetReachRadius = 150;
    [Export]
    public float targetReachDelay = 3;
    
    [Export]
    public float maxVelocity = 250;
    [Export]
    public float maxAngularVelocity = float.Pi;
    [Export]
    public float acceleration = 250;
}
