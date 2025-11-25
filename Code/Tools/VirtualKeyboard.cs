using Godot;
using System;
using System.Collections.Generic;

public partial class VirtualKeyboard : Control
{
    public Action OnSubmit;
    public string Text => _display.Text;
    
    [Export] private LineEdit _display;
    [Export] private VBoxContainer _keysContainer;

    private List<List<Button>> _buttonGrid = new List<List<Button>>();
    
    private int _currentRow = 0;
    private int _currentCol = 0;

    private readonly string[][] _layout = new string[][]
    {
        new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
        new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L", "Ñ" },
        new string[] { "Z", "X", "C", "V", "B", "N", "M", "BORRAR", "ENVIAR" }
    };

    public override void _Ready()
    {
        GenerateKeyboard();
        
        UpdateFocus();
    }
    
    public override void _Process(double delta) 
    {
        CheckInput();
    }

    private void GenerateKeyboard()
    {
        _buttonGrid.Clear();

        foreach (var rowKeys in _layout)
        {
            var rowHBox = new HBoxContainer();
            rowHBox.Alignment = BoxContainer.AlignmentMode.Center;
            _keysContainer.AddChild(rowHBox);

            var currentRowList = new List<Button>();

            foreach (var keyTag in rowKeys)
            {
                var btn = new Button();
                if (keyTag == "SPACE") 
                {
                    btn.CustomMinimumSize = new Vector2(100, 25);
                    btn.Text = "SPACE";
                }
                else if (keyTag == "BORRAR")
                {
                    btn.CustomMinimumSize = new Vector2(25, 25);
                    btn.Text = "<";
                }
                else if (keyTag == "ENVIAR")
                {
                    btn.CustomMinimumSize = new Vector2(50, 25);
                    btn.Text = "ENVIAR";
                }
                else 
                {
                    btn.CustomMinimumSize = new Vector2(25, 25);
                    btn.Text = keyTag;
                }

                btn.FocusMode = FocusModeEnum.All; 

                string charToAppend = (keyTag == "SPACE") ? " " : keyTag;
                btn.Pressed += () => OnKeyPressed(charToAppend);

                rowHBox.AddChild(btn);
                
                currentRowList.Add(btn);
            }
            
            _buttonGrid.Add(currentRowList);
        }
    }

    public void CheckInput()
    {
        if (Input.IsActionJustReleased("ui_right"))
        {
            Navigate(0, 1);
        }
        else if (Input.IsActionJustReleased("ui_left"))
        {
            Navigate(0, -1);
        }
        else if (Input.IsActionJustReleased("ui_down"))
        {
            Navigate(1, 0);
        }
        else if (Input.IsActionJustReleased("ui_up"))
        {
            Navigate(-1, 0);
        }
    }

    private void Navigate(int rowDir, int colDir)
    {
        int newRow = Mathf.Clamp(_currentRow + rowDir, 0, _buttonGrid.Count - 1);
        int keysInNewRow = _buttonGrid[newRow].Count;
        int newCol = _currentCol + colDir;

        if (rowDir != 0) 
        {
            newCol = Mathf.Clamp(_currentCol, 0, keysInNewRow - 1);
            //if (newRow == _layout.Length - 1) 
                //newCol = 0;
        }
        else
        {
            newCol = Mathf.Clamp(newCol, 0, keysInNewRow - 1);
        }

        _currentRow = newRow;
        _currentCol = newCol;
        
        UpdateFocus();
    }

    private void UpdateFocus()
    {
        var targetBtn = _buttonGrid[_currentRow][_currentCol];
        targetBtn.GrabFocus();
    }
    
    private void OnKeyPressed(string character)
    {
        if (character == "BORRAR")
        {
            _display.Text = _display.Text.Remove(_display.Text.Length - 1);
        } else if (character == "ENVIAR")
        {
            if (!string.IsNullOrEmpty(_display.Text))
                OnSubmit?.Invoke();
        }
        else
        {
            _display.Text += character;
            _display.CaretColumn = _display.Text.Length;
        }
    }
}