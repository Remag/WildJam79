using Godot;
using System;

[GlobalClass]
public partial class TargetBulletSpawner : BulletSpawner {
    [Export]
    private PackedScene _bulletPrefab;

    public override void SpawnBullets( Node2D shootAnchor, Vector2 targetCanvasPos, bool isEnemyBullet )
    {
        var bullet = _bulletPrefab.Instantiate<BasicBullet>();
        var bulletDir = targetCanvasPos - Game.Camera.GetCanvasTransform() * shootAnchor.GlobalPosition;
        bullet.Rotation = bulletDir.Angle();
        bullet.GlobalPosition = shootAnchor.GlobalPosition;
        bullet.SetIsEnemyBullet( isEnemyBullet );
        Game.Field.AddChild( bullet );
    }
}
