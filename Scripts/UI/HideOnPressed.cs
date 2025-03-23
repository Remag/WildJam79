using Godot;
using System;

public partial class HideOnPressed : Button
{
    public override void _Ready()
    {
        Pressed += () => Hide();
    }
}
