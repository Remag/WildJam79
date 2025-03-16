using Godot;
using System;

public partial class PlayerBlobCore : Node2D {
    [Export]
    private int _weaponHeatGenSpeed = 100;
    [Export]
    private double _attackDelaySec = 0.2;
    [Export]
    private PackedScene _bulletPrefab;

    private double _weaponHeat = 0;
    private double _currentDelay = 0;

    public override void _Process( double delta )
    {
        _weaponHeat -= 30 * delta;
        _weaponHeat = Math.Max( _weaponHeat, 0 );
        Modulate = Colors.White.Lerp( Colors.Red, (float)_weaponHeat / 100 );
    }

    public void UpdateShooting( double delta )
    {
        if( _weaponHeat >= 100 ) {
            return;
        }
        _weaponHeat += _weaponHeatGenSpeed * delta;

        _currentDelay -= delta;
        if( _currentDelay <= 0 ) {
            _currentDelay = _attackDelaySec;
            spawnBullet();
        }
    }

    private void spawnBullet()
    {
        var bullet = _bulletPrefab.Instantiate<BasicBullet>();
        var mousePos = GetViewport().GetMousePosition();
        var bulletDir = mousePos - GlobalPosition;
        bullet.Rotation = bulletDir.Angle();
        bullet.GlobalPosition = GlobalPosition;
        bullet.SetCollisionParams( Game.PlayerBulletLayer, Game.EnemyShipLayer );
        Game.Map.AddChild( bullet );
    }
}
