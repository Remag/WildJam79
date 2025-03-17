using Godot;
using System;

[GlobalClass]
public partial class BackgroundSelection : Resource {
[Export]
public Godot.Collections.Array<Texture2D> Stars;
[Export]
public Godot.Collections.Array<Texture2D> Nebulae;
}
