using Godot;
using System;

public partial class HeroWeaponCore : Node2D {
    [Export]
    private Godot.Collections.Array<int> _levelReqs;
    [Export]
    private Godot.Collections.Array<PlayerWeapon> _levelNodes;

    [Export]
    private AudioStreamPlayer _shootSoundPlayer;

    public PackedScene SrcCore { get; private set; }

    public int _currentExp = 0;
    public int CurrentLevel = 0;

    public void Initialize( PackedScene src )
    {
        SrcCore = src;
    }

    public override void _Ready()
    {
        foreach( var node in _levelNodes ) {
            node.Visible = false;
        }

        _levelNodes[CurrentLevel].Visible = true;
    }

    public int GetRequiredExp()
    {
        if( CurrentLevel >= _levelReqs.Count ) {
            return -1;
        }
        var currentReq = _levelReqs[CurrentLevel];
        return currentReq - _currentExp;
    }

    public void GainExp( int exp )
    {
        if( CurrentLevel >= _levelReqs.Count ) {
            return;
        }

        var currentReq = _levelReqs[CurrentLevel];
        _currentExp += exp;
        if( _currentExp >= currentReq ) {
            _currentExp -= currentReq;
            if( Game.ResetXpAfterAssimilate ) {
                _currentExp = 0;
            }
            advanceLevel();
        }
    }

    private void advanceLevel()
    {
        _levelNodes[CurrentLevel].Visible = false;
        CurrentLevel++;
        _levelNodes[CurrentLevel].Visible = true;
    }

    public void UpdateShooting( double delta )
    {
        var shooted = _levelNodes[CurrentLevel].UpdateShooting( delta );
        if( shooted )
            _shootSoundPlayer.Play();
    }

    public SavedState SaveState()
    {
        var state = new SavedState();
        state._currentExp = _currentExp;
        state._currentLevel = CurrentLevel;
        return state;
    }

    public void RestoreState( SavedState state )
    {
        _levelNodes[CurrentLevel].Visible = false;
        _currentExp = state._currentExp;
        CurrentLevel = state._currentLevel;
        _levelNodes[CurrentLevel].Visible = true;
    }

    public class SavedState {
        public int _currentExp = 0;
        public int _currentLevel = 0;

    }
}
