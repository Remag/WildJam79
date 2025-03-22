using Godot;

public partial class TentaclePlayer : Tentacle {
    [Export]
    private float _mouseGravity = 5.0f;
    [Export]
    public float TentacleMaxDistance = 100;
    [Export]
    public double TentacleMaxTime = 6;
    [Export]
    public int MinSizeLevel = 0;

    private double _currentExtendTime = 0;
    private Vector2 _currentVelocity;

    public void Initialize( Player player )
    {
        var endAnchor = _tentacleLine.EndAnchor;
        endAnchor.GlobalPosition = player.GlobalPosition;
    }

    public override void _Ready()
    {
        var canvasMousePos = GetViewport().GetMousePosition();
        var globalMousePos = Game.Camera.GetCanvasTransform().AffineInverse() * canvasMousePos;
        var endAnchor = _tentacleLine.EndAnchor;
        var dirDelta = globalMousePos - endAnchor.GlobalPosition;

        _currentVelocity = new Vector2( _tentacleExtendSpeed, 0 ).Rotated( dirDelta.Angle() + Mathf.DegToRad( Rng.Choose( [-15.0f, 15.0f] ) ) );

        base._Ready();
    }

    public override void _PhysicsProcess( double delta )
    {
        _tentacleLine.StartAnchor.GlobalPosition = Game.Player.GlobalPosition;
        var deltaF = (float)delta;
        switch( _currentMode ) {
            case TentacleMode.Extend:
                updateExtend( deltaF );
                break;
        }

        base._PhysicsProcess( delta );
    }

    private void updateExtend( float delta )
    {
        var canvasMousePos = GetViewport().GetMousePosition();
        var globalMousePos = Game.Camera.GetCanvasTransform().AffineInverse() * canvasMousePos;
        var endAnchor = _tentacleLine.EndAnchor;
        var dirDelta = globalMousePos - endAnchor.GlobalPosition;
        _currentVelocity += dirDelta * _mouseGravity * delta;
        _currentVelocity = _currentVelocity.Normalized() * _tentacleExtendSpeed;
        endAnchor.GlobalPosition += _currentVelocity * delta;

        _currentExtendTime += delta;
        if( _currentExtendTime >= TentacleMaxTime || ( _tentacleLine.EndAnchor.GlobalPosition - _tentacleLine.StartAnchor.GlobalPosition ).LengthSquared() >= TentacleMaxDistance * TentacleMaxDistance ) {
            SetShrink();
        }
    }

    public void OnAreaCollision( Node node )
    {
        if( _currentMode == TentacleMode.Shrink ) {
            return;
        }

        switch (node)
        {
            case Shield enemyShield:
                if( enemyShield.EnemyShip.SizeLevel >= MinSizeLevel ) {
                    AbortExtend();
                    Game.Field.WorldAudioManager.ShieldReflectNoDamageSoundPlay();
                }
                break;
            case EnemyShip enemyShip:
                if( enemyShip.SizeLevel >= MinSizeLevel ) {
                    Attach( enemyShip );
                    enemyShip.OnTentacleCollision();
                }
                break;
            case FoodSource foodSource:
                Attach( foodSource );
                foodSource.OnTentacleCollision();
                break;
            case BasicBullet missile:
                missile.HandleDestroy( true );
                break;
        }
    }
}
