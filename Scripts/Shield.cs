using Godot;
using System;

public partial class Shield : Area2D {
    [Export] private int _maxHp = 5;
    [Export] private float _shieldRegenTime = 10;
    
    [Signal]
    public delegate void ShieldDestroyEventHandler();
    
    [Signal]
    public delegate void ShieldRegenerateEventHandler();
    
    
    private int _currentHp;
    private float _currentHpRegenTime;
    public override void _Ready()
    {
        _currentHp = _maxHp;
    }

    public override void _PhysicsProcess( double delta )
    {
        if( _currentHp == 0 ) {
            _currentHpRegenTime += (float)delta;
            if( _currentHpRegenTime > _shieldRegenTime ) {
                _currentHpRegenTime = 0;
                _currentHp = _maxHp;
                Monitorable = true;
                SetDeferred( Area2D.PropertyName.Monitorable, true );
                Visible = true;
                Modulate = Colors.White;
                EmitSignal( SignalName.ShieldRegenerate );
            }
        }
    }

    public void OnBulletCollision(int damage)
    {
        _currentHp -= damage;
        if( _currentHp <= 0 ) {
            _currentHp = 0;
            SetDeferred( Area2D.PropertyName.Monitorable, false );
            Visible = false;
            EmitSignal( SignalName.ShieldDestroy );
        }
        Modulate = Colors.Transparent.Lerp( Colors.White, (float)_currentHp / _maxHp );
    }
}
