using Godot;
using System;

public partial class HudScroll : Sprite2D
{
    [Export]
    public float Speed = 25f;

    public override void _Process(double delta)
    {
        Rect2 currentRegion = RegionRect;
        currentRegion.Position += new Vector2((float)(Speed * delta), 0);
        RegionRect = currentRegion;
    }
}
