using Godot;

public partial class Boundary_tp_ : Area2D
{
    [Export] private float respawnX = 0f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            Vector2 respawnPosition = new Vector2(respawnX, player.GlobalPosition.Y);
            player.GlobalPosition = respawnPosition;
        }
    }
}