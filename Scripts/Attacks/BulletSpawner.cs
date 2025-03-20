using Godot;
using System;

[GlobalClass]
public abstract partial class BulletSpawner : Resource
{
    public abstract void SpawnBullets( Node2D shootAnchor, Vector2 targetCanvasPos, bool isEnemyBullet );
}
