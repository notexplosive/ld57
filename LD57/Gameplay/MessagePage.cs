using System;
using System.Collections.Generic;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class MessagePage
{
    private List<List<TileState>> _lines = new();
    private Color _currentColor = Color.White;
    public int Width { get; private set; }
    public int Height => _lines.Count;

    public void AddLine(string rawText)
    {
        var line = new List<TileState>();

        var isReadingCommand = false;
        for (var index = 0; index < rawText.Length; index++)
        {
            var character = rawText[index];
            if (character == '{' && isReadingCommand == false)
            {
                isReadingCommand = true;
                var command = GetCommandSubstring(rawText, index+1);
                var splitCommand = command.Split(':');
                if (splitCommand[0] == "c")
                {
                    if (splitCommand.Length == 2)
                    {
                        _currentColor = ResourceAlias.Color(splitCommand[1]);
                    }
                }

                if (splitCommand[0] == "i")
                {
                    if (splitCommand.Length == 3)
                    {
                        line.Add(TileState.Sprite(LdResourceAssets.Instance.Sheets[splitCommand[1]], int.Parse(splitCommand[2]), _currentColor));
                    }
                }
            }

            if (character == '}')
            {
                isReadingCommand = false;
                continue;
            }

            if (isReadingCommand)
            {
                continue;
            }

            line.Add(TileState.StringCharacter(rawText.Substring(index, 1), _currentColor));
        }

        Width = Math.Max(Width, line.Count);
        
        _lines.Add(line);
    }

    private string GetCommandSubstring(string rawText, int start)
    {
        var end = rawText.Length;
        for (var i = start; i < rawText.Length; i++)
        {
            var character = rawText[i];
            if (character == '}')
            {
                end = i;
                break;
            }
        }

        return rawText.Substring(start, end - start);
    }

    public void PaintToScreen(GridPosition topLeft, AsciiScreen screen)
    {
        var verticalOffset = 0;
        foreach (var line in _lines)
        {
            var horizontalOffset = 0;
            foreach (var tile in line)
            {
                screen.PutTile(topLeft + new GridPosition(horizontalOffset, verticalOffset), tile);
                horizontalOffset++;
            }

            verticalOffset++;
        }
    }

    public bool HasContent()
    {
        return _lines.Count > 0;
    }
}
