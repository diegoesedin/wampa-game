using Godot;

public partial class Boundary_tp : Area2D
{
    [Export] private float respawnY = 0f; 

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            Vector2 respawnPosition = new Vector2(player.GlobalPosition.X, respawnY);
            player.GlobalPosition = respawnPosition;
            player.Velocity = new Vector2(player.Velocity.X, 0);
        }
    }
}