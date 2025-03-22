using Godot;
using System;

[GlobalClass]
public partial class TargetBulletSpawner : BulletSpawner {
    [Export]
    private PackedScene _bulletPrefab;
    [Export]
    private float _rngAngleDeg = 0;

    public override void SpawnBullets( Node2D shootAnchor, Node2D spawnPoint, Vector2 targetGlobalPos, bool isEnemyBullet )
    {
        var bullet = _bulletPrefab.Instantiate<BasicBullet>();
        var bulletDir = targetGlobalPos - shootAnchor.GlobalPosition;
        var rngAngleRad = Mathf.DegToRad( _rngAngleDeg );
        bullet.Rotation = bulletDir.Angle() + Rng.RandomRange( -rngAngleRad, rngAngleRad );
        bullet.GlobalPosition = spawnPoint.GlobalPosition;
        bullet.SetIsEnemyBullet( isEnemyBullet );
        Game.Field.AddChild( bullet );
    }
}
