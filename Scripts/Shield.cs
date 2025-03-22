using Godot;
using System;
using WildJam78.Scripts;
using WildJam78.Scripts.Shield;

public partial class Shield : ParentArea2D {
    [Export]
    private ShieldConfig _config = new();
    [Export]
    private AudioStreamPlayer _shieldDropSoundPlayer;
    [Export]
    private AudioStreamPlayer _shieldReflectSoundPlayer;

    [Export]
    private Sprite2D _baseSprite;
    [Export]
    private Sprite2D _surfaceSprite;
    [Export]
    private PackedScene _shieldEffect;

    [Export]
    private float _shieldEffectMaxDelay = 0.2f;
    private double _shieldEffectDelay = 0;

    [Signal]
    public delegate void ShieldDestroyEventHandler();

    private int _currentHp;
    private float _currentHpRegenTime;
    public override void _Ready()
    {
        _currentHp = _config.maxHp;
        RealParent = this;
        _shieldEffectDelay = _shieldEffectMaxDelay;
    }

    public override void _Process( double delta )
    {
        _shieldEffectDelay -= delta;
    }

    public void OnBulletCollision( int damage, Node2D source )
    {
        if( _shieldEffectDelay > 0 ) {
            return;
        }
        _shieldEffectDelay = _shieldEffectMaxDelay;

        var circleRadius = 50;
        var sourceDir = ( Position - ToLocal( source.GlobalPosition ) ).Normalized();
        
        var startPoint = _surfaceSprite.Position - sourceDir * circleRadius;
        var effect = _shieldEffect.Instantiate<Node2D>();
        _surfaceSprite.AddChild( effect );
        effect.Position = startPoint;
        effect.Rotation = sourceDir.Angle();
        var posTween = effect.CreateTween();

        posTween.TweenProperty( effect, "position", startPoint + sourceDir * circleRadius * 3, 0.4 ).SetEase( Tween.EaseType.InOut );
        delayDeleteEffect( effect );

        _currentHp -= damage;
        if( _currentHp <= 0 ) {
            _currentHp = 0;
            SetDeferred( Area2D.PropertyName.Monitorable, false );
            Visible = false;
            EmitSignal( SignalName.ShieldDestroy );
            _shieldDropSoundPlayer.Play();
        } else {
            _shieldReflectSoundPlayer.Play();
        }
        _baseSprite.Modulate = Colors.Transparent.Lerp( Colors.White, (float)_currentHp / _config.maxHp );
    }

    private async void delayDeleteEffect( Node effect )
    {
        await ToSignal( GetTree().CreateTimer( 0.75 ), "timeout" );
        if( IsInstanceValid( effect ) ) {
            effect.QueueFree();
        }
    }

}
