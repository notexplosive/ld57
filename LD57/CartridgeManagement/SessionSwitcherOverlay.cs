using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD57.Editor;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.CartridgeManagement;

public class SessionSwitcherOverlay
{
    private readonly TextInputElement _textInput;
    private readonly List<string> _logBuffer = new();

    public SessionSwitcherOverlay()
    {
        _textInput = new TextInputElement(new GridPosition(0, 5), string.Empty);

        _textInput.Submitted += OnSubmitText;
    }

    public bool Visible { get; private set; }

    private static Color BackgroundColor => ResourceAlias.Color("abyss");

    public void Log(string message)
    {
        _logBuffer.Add(message);
    }

    private void OnSubmitText(string text)
    {
        _logBuffer.Add(">" + text);

        var response = new ConsoleCommandResponse(this);
        CommandSent?.Invoke(text, response);

        if (response.Status == ConsoleCommandResponse.StatusType.Success)
        {
            Close();
        }
        
        if(response.Status == ConsoleCommandResponse.StatusType.Unrecognized)
        {
            _logBuffer.Add($"Unrecognized Command: {text}");
        }
        
        _textInput.Clear();
    }

    public event Action<string, ConsoleCommandResponse>? CommandSent;

    public void ToggleVisible()
    {
        Visible = !Visible;
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        _textInput.UpdateKeyboardInput(input.Keyboard);
        var enteredCharacters = input.Keyboard.GetEnteredCharacters(true);
        _textInput.OnTextInput(enteredCharacters);
    }

    public void Update(AsciiScreen screen, float dt)
    {
        // colors are not loaded at constructor-time so we need to wait until now
        _textInput.SetBackgroundColor(BackgroundColor);

        var height = 5;
        screen.PutFilledRectangle(TileState.BackgroundOnly(BackgroundColor, 1f),
            new GridRectangle(0, 0, screen.Width, height));

        _textInput.PutSubElementOnScreen(screen, null);

        var i = 0;
        for (var y = height - 1; y >= 0; y--)
        {
            var index = _logBuffer.Count - 1 - i;
            if (_logBuffer.IsValidIndex(index))
            {
                var message = _logBuffer[index];
                screen.PutString(new GridPosition(0,y),message, ResourceAlias.Color("grayed-out"), BackgroundColor);
            }

            i++;
        }
    }

    public void Close()
    {
        Visible = false;
    }
}