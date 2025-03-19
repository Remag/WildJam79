using Godot;
using System;

public partial class HeroWeaponCore : Node2D {
    [Export]
    private Godot.Collections.Array<int> _levelReqs;
    [Export]
    private Godot.Collections.Array<PlayerWeapon> _levelNodes;

    public PackedScene SrcCore { get; private set; }

    private int _currentExp = 0;
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
        _levelNodes[0].Visible = true;
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
}
