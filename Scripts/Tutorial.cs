using Godot;
using System;

public partial class Tutorial : Node2D {
    [Export]
    private AnimationPlayer _animations;

    [Export]
    private PackedScene _ship;
    private EnemyShip _activeShip;

    public enum Stage {
        Start,
        ConsumeNear,
        ConsumeFar,
        WaitShipSpawn,
        FightShip,
        WaitShoot,
        End
    }
    public Stage CurrentStage = Stage.Start;

    public void Start()
    {
        _animations.Play( "ShowConsumeNear" );
        CurrentStage = Stage.ConsumeNear;
    }

    public override void _PhysicsProcess( double delta )
    {
        switch( CurrentStage ) {
            case Stage.ConsumeNear:
                updateConsumeNear();
                break;
            case Stage.ConsumeFar:
                updateConsumeFar();
                break;
            case Stage.FightShip:
                updateFight();
                break;
            case Stage.WaitShoot:
                updateShoot();
                break;
        }
    }

    private void updateConsumeNear()
    {
        var debris = GetTree().GetNodesInGroup( "NearDebris" );
        if( debris.Count > 0 ) {
            return;
        }

        Game.Player.SetMoveEnabled( true );
        Game.Player.CanDie = false;
        var farDebris = GetTree().GetNodesInGroup( "FarDebris" );
        foreach( SpaceDebris far in farDebris ) {
            far.ShowIndicator = true;
        }
        _animations.Play( "ShowConsumeFar" );
        CurrentStage = Stage.ConsumeFar;
    }

    private void updateConsumeFar()
    {
        var farDebris = GetTree().GetNodesInGroup( "FarDebris" );
        if( farDebris.Count > 0 ) {
            return;
        }
        _animations.Play( "ShowFight" );
        CurrentStage = Stage.WaitShipSpawn;
    }

    public void SpawnEnemy()
    {
        _activeShip = Game.Field.SpawnEnemy( _ship );
        CurrentStage = Stage.FightShip;
    }

    private void updateFight()
    {
        if( IsInstanceValid( _activeShip ) ) {
            return;
        }
        _animations.Play( "ShowWarp" );
        CurrentStage = Stage.WaitShoot;
    }

    private void updateShoot()
    {
        var bullets = GetTree().GetNodesInGroup( "ClearOnLevelClear" );
        foreach( var bullet in bullets ) {
            if( bullet is BasicBullet basicBullet && !basicBullet.IsEnemyBullet ) {
                _animations.Play( "Finalize" );
                CurrentStage = Stage.End;
                return;
            }
        }

    }

    public void Cleanup()
    {
        Game.Player.CanDie = true;
        Game.Field.EndTutorial();
    }
}
