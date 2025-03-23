using Godot;
using System;

public partial class PlayerWeapon : Node2D {
    [Export]
    private double _attackDelaySec = 0.2;
    [Export]
    private BulletSpawner _bulletSpawner;
    [Export]
    private Node2D _spawnPoint;
    
    private double _currentDelay = 0;

    public override void _PhysicsProcess( double delta )
    {
        var mousePos = Game.Camera.GetCanvasTransform().AffineInverse() * GetViewport().GetMousePosition();
        GlobalRotation = (mousePos - GlobalPosition).Angle() + float.Pi / 2;
    }

    public bool UpdateShooting( double delta )
    {
        _currentDelay -= delta;
        
        if( _currentDelay <= 0 ) {
            _currentDelay = _attackDelaySec;
            
            var mousePos = Game.Camera.GetCanvasTransform().AffineInverse() * GetViewport().GetMousePosition();
            _bulletSpawner.SpawnBullets( this, _spawnPoint ?? this, mousePos, isEnemyBullet:false );
            return true;
        }
        return false;
    }
}
