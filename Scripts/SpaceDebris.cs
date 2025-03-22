using Godot;
using System;
using WildJam78.Scripts;

public partial class SpaceDebris : FoodSource {

    [Export]
    private float _spriteScale = 1;
    [Export]
    private Sprite2D _sprite;
    [Export]
    private float _driftAngularSpeed = 10;
    [Export]
    private float _driftMaxSpeed = 5;
    [Export]
    private OffscreenIndicator _offscreenIndicator = null;

    public bool ShowIndicator = false;

    private Vector2 _driftVelocity;

    public override Node2D GetTentacleAnchor()
    {
        return this;
    }

    public override void OnBroughtToPlayer( Tentacle tentacle )
    {
        Game.Player?.Assimilate( this, tentacle );
        QueueFree();
    }

    public override void OnTentacleCollision()
    {
    }

    public override bool TryTentaclePull( Tentacle tentacle )
    {
        CollisionLayer = 0;
        return true;
    }

    public override void _Ready()
    {
        var randomDir = new Vector2( 1, 0 ).Rotated( Rng.RandomRange( 0, 2 * Mathf.Pi ) );
        _driftVelocity = Rng.RandomRange( 0, _driftMaxSpeed ) * randomDir;
        _sprite.Scale = new Vector2( _spriteScale, _spriteScale );

        if( _offscreenIndicator != null ) {
            RemoveChild( _offscreenIndicator );
            CallDeferred( MethodName.addIndicator );
        }
    }

    private void addIndicator()
    {
        GetParent().AddChild( _offscreenIndicator );
    }

    public override void _PhysicsProcess( double delta )
    {
        handleIndicator();
    }

    private void handleIndicator()
    {
        if( _offscreenIndicator == null || !ShowIndicator ) {
            return;
        }
        var cameraRect = Game.Camera.GetCanvasTransform().AffineInverse() * GetViewportRect();
        var position = GlobalPosition;
        if( !cameraRect.HasPoint( position ) ) {
            _offscreenIndicator.ShowIndicator();
            _offscreenIndicator.Rotation = ( position - cameraRect.GetCenter() ).Angle();
            var newPosition = position.Clamp( cameraRect.Position, cameraRect.End );
            _offscreenIndicator.GlobalPosition = newPosition;
        } else {
            _offscreenIndicator.HideIndicator();
        }
    }

    public override void _IntegrateForces( PhysicsDirectBodyState2D state )
    {
        state.AngularVelocity = Mathf.DegToRad( _driftAngularSpeed );
    }

}
