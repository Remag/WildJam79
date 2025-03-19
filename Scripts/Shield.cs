using Godot;
using System;
using WildJam78.Scripts.Shield;

public partial class Shield : Area2D {
    [Export]
    private ShieldConfig _config = new();
    [Export]
    private AudioStreamPlayer _shieldDropSoundPlayer;
    [Export]
    private AudioStreamPlayer _shieldReflectSoundPlayer;

    [Signal]
    public delegate void ShieldDestroyEventHandler();

    [Signal]
    public delegate void ShieldRegenerateEventHandler();

    private int _currentHp;
    private float _currentHpRegenTime;
    public override void _Ready()
    {
        _currentHp = _config.maxHp;
    }

    public override void _PhysicsProcess( double delta )
    {
        if( _currentHp == 0 && _config.shieldRegenHp > 0 ) {
            _currentHpRegenTime += (float)delta;
            if( _currentHpRegenTime > _config.shieldRegenTime ) {
                _currentHpRegenTime = 0;
                _currentHp = _config.shieldRegenHp;
                SetDeferred( Area2D.PropertyName.Monitorable, true );
                Visible = true;
                Modulate = Colors.White;
                EmitSignal( SignalName.ShieldRegenerate );
            }
        }
    }

    public void OnBulletCollision( int damage )
    {
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
        Modulate = Colors.Transparent.Lerp( Colors.White, (float)_currentHp / _config.maxHp );
    }
}
