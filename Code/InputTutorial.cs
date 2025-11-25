using Godot;
using System.Collections.Generic;

public partial class InputTutorial : Node
{
    private GridContainer ButtonGrid;
    private Node2D ContinueNode;

    private Dictionary<string, Button> buttons = new();
    private HashSet<string> pressedInputs = new();
    
    private readonly Dictionary<string, string> inputMap = new()
    {
        { "A", "ui_left" },
        { "D", "ui_right" },
        { "W", "jump" },
        { "S", "crouch" },
        { "LEFT", "attack_left" },
        { "RIGHT", "attack_right" }
    };

    public override void _Ready()
    {
        ButtonGrid = GetNode<GridContainer>("InputCheck");
        ContinueNode = GetNode<Node2D>("ContinueNode");

        foreach (Node child in ButtonGrid.GetChildren())
        {
            if (child is Button btn)
            {
                buttons[btn.Name] = btn;
                btn.Disabled = true;
            }
        }

        if (ContinueNode != null)
            ContinueNode.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsPressed() && !@event.IsEcho())
        {
            foreach (var kvp in inputMap)
            {
                if (Input.IsActionJustPressed(kvp.Value))
                {
                    MarkInputPressed(kvp.Key);
                }
            }
        }
    }

    private void MarkInputPressed(string buttonName)
    {
        if (buttons.ContainsKey(buttonName) && !pressedInputs.Contains(buttonName))
        {
            pressedInputs.Add(buttonName);
            var btn = buttons[buttonName];
            btn.ButtonPressed = true;
            btn.Modulate = new Color(0.5f, 1f, 0.5f); // Verde claro

            if (buttonName == "A" || buttonName == "D")
            {
                AudioManager.Instance.PlaySFX("enemy_death");         
            }
            
            if (pressedInputs.Count == buttons.Count)
            {
                CompleteTutorial();
            }
        }
    }

    private void CompleteTutorial()
    {
        ButtonGrid.Visible = false;
        if (ContinueNode != null)
        {
            ContinueNode.Visible = true;
            
            var audioPlayer = ContinueNode.GetNode<AudioStreamPlayer>("AudioStreamPlayer");
            if (audioPlayer != null)
                audioPlayer.Play();
        }
    }
}