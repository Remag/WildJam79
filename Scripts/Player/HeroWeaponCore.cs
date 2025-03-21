using Godot;
using System;

public partial class HeroWeaponCore : Node2D {
    [Export]
    private Godot.Collections.Array<int> _levelReqs;
    [Export]
    private Godot.Collections.Array<PlayerWeapon> _levelNodes;

    public PackedScene SrcCore { get; private set; }

    public int _currentExp = 0;
    public int _currentLevel = 0;

    public void Initialize( PackedScene src )
    {
        SrcCore = src;
    }

    public override void _Ready()
    {
        foreach( var node in _levelNodes ) {
            node.Visible = false;
        }

        _levelNodes[_currentLevel].Visible = true;
    }

    public void GainExp( int exp )
    {
        if( _currentLevel >= _levelReqs.Count ) {
            return;
        }

        var currentReq = _levelReqs[_currentLevel];
        _currentExp += exp;
        if( _currentExp >= currentReq ) {
            _currentExp -= currentReq;
            advanceLevel();
        }
    }

    private void advanceLevel()
    {
        _levelNodes[_currentLevel].Visible = false;
        _currentLevel++;
        _levelNodes[_currentLevel].Visible = true;
    }

    public void UpdateShooting( double delta )
    {
        _levelNodes[_currentLevel].UpdateShooting( delta );
    }

    public SavedState SaveState()
    {
        var state = new SavedState();
        state._currentExp = _currentExp;
        state._currentLevel = _currentLevel;
        return state;
    }

    public void RestoreState( SavedState state )
    {
        _levelNodes[_currentLevel].Visible = false;
        _currentExp = state._currentExp;
        _currentLevel = state._currentLevel;
        _levelNodes[_currentLevel].Visible = true;
    }

    public class SavedState {
        public int _currentExp = 0;
        public int _currentLevel = 0;

    }
}
