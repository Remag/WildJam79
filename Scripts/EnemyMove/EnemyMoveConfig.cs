using Godot;

namespace WildJam78.Scripts.EnemyMove;

[GlobalClass]
public partial class EnemyMoveConfig : Resource {
    [Export]
    public float aiTurnDuration = 1;
    [Export]
    public float aiAccelDuration = 3;
    [Export]
    public float aiMoveDuration = 5;
    [Export]
    public float maxVelocityPxSec = 4;
    [Export]
    public float maxAccelPxSec = 5;
    [Export]
    public float rotationSpeedRadSec = 0.1F;
    [Export]
    public float aiTurnVarianceRad = 0.5F;
}