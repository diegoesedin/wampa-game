using Godot;

public partial class MovingPlatform : Path2D
{
    [Export] private float speed = 50f;
    [Export] private bool loop = true;
    [Export] private bool pingPong = false;
    [Export] private int spriteFrame = 0;

    private PathFollow2D follower;
    private AnimatedSprite2D sprite;
    private AnimatableBody2D body;
    private bool movingForward = true;

    public override void _Ready()
    {
        follower = GetNode<PathFollow2D>("PathFollow2D");
        follower.Loop = loop;

        body = GetNode<AnimatableBody2D>("AnimatableBody2D");
        body.SyncToPhysics = true;

        sprite = GetNode<AnimatedSprite2D>("AnimatableBody2D/AnimatedSprite2D");
        sprite.Stop();
        sprite.Frame = spriteFrame;
    }

    public override void _PhysicsProcess(double delta)
    {
        float step = speed * (float)delta;

        if (pingPong)
        {
            if (movingForward)
            {
                follower.Progress += step;
                if (follower.ProgressRatio >= 1.0f)
                {
                    follower.ProgressRatio = 1.0f;
                    movingForward = false;
                }
            }
            else
            {
                follower.Progress -= step;
                if (follower.ProgressRatio <= 0.0f)
                {
                    follower.ProgressRatio = 0.0f;
                    movingForward = true;
                }
            }
        }
        else
        {
            follower.Progress += step;
        }
    }

}