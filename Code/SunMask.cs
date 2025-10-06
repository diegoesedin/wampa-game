using Godot;

public partial class SunMask : Node2D
{
    private AnimationPlayer anim;
    private Area2D area;

    public override void _Ready()
    {
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("Floating");

        area = GetNode<Area2D>("Node2D/Area2D");
        area.BodyEntered += OnBodyEntered;

    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            var playerScript = body as Player;
            playerScript?.PickUpMask();

            QueueFree();
        }
    }

}
