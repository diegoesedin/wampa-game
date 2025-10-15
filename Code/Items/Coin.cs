using Godot;

public partial class Coin : Node2D
{
    [Signal] public delegate void CoinCollectedEventHandler();

    private Area2D area;

    public override void _Ready()
    {
        area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {      
        if (!body.IsInGroup("Player"))
            return;
        
        EmitSignal(nameof(CoinCollected)); 
        QueueFree(); 
    }
}
