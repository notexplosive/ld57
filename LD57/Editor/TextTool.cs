using System;
using System.Collections.Generic;
using System.Text;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class CanvasTextTool : IEditorTool
{
    private readonly List<char> _buffer = new();
    private readonly CanvasBrushFilter _canvasBrushFilter;
    private readonly CanvasEditorSurface _canvasSurface;
    private float _lastTimeStamp;
    private GridPosition? _startPosition;

    public CanvasTextTool(CanvasEditorSurface canvasSurface, CanvasBrushFilter canvasBrushFilter)
    {
        _canvasSurface = canvasSurface;
        _canvasBrushFilter = canvasBrushFilter;
    }

    public TileState TileStateInToolbar { get; } = TileState.Sprite(ResourceAlias.Tools, 15);

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        if (!_startPosition.HasValue)
        {
            return original.WithSprite(ResourceAlias.Tools, 15);
        }

        return original with {BackgroundColor = Color.White, BackgroundIntensity = 0.5f};
    }

    public string Status()
    {
        return "Text";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard, GridPosition? hoveredWorldPosition)
    {
        if (_startPosition.HasValue)
        {
            if (inputKeyboard.GetButton(Keys.Escape).WasPressed)
            {
                ClearState();
            }

            foreach (var character in inputKeyboard.GetEnteredCharacters(true))
            {
                _lastTimeStamp = Client.TotalElapsedTime;
                if (!char.IsControl(character))
                {
                    _buffer.Add(character);
                }
                else
                {
                    if (character == '\b')
                    {
                        if (_buffer.Count > 0)
                        {
                            _buffer.RemoveAt(_buffer.Count - 1);
                        }
                    }

                    if (character == '\r')
                    {
                        PutStringAt(_buffer, _startPosition.Value);
                        ClearState();
                    }
                }
            }
        }
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (position.HasValue && mouseButton == MouseButton.Left)
        {
            _lastTimeStamp = Client.TotalElapsedTime;
            _startPosition = position.Value;
        }
    }

    public void PaintToWorld(AsciiScreen screen)
    {
        if (_startPosition != null)
        {
            screen.PutString(_startPosition.Value, BufferToString());

            if (MathF.Sin(CurrentTimer() * 5) > 0)
            {
                screen.PutTile(_startPosition.Value + new GridPosition(_buffer.Count, 0),
                    TileState.Sprite(ResourceAlias.Tools, 19));
            }
            else
            {
                screen.PutTile(_startPosition.Value + new GridPosition(_buffer.Count, 0),
                    TileState.Sprite(ResourceAlias.Tools, 20));
            }
        }
    }

    private void ClearState()
    {
        _buffer.Clear();
        _startPosition = null;
    }

    private void PutStringAt(List<char> characters, GridPosition startPosition)
    {
        var xOffset = 0;
        foreach (var character in characters)
        {
            var targetPosition = startPosition + new GridPosition(xOffset, 0);
            var existingTile = _canvasSurface.Data.InkAt(targetPosition);
            var tileToPaint = _canvasBrushFilter.GetFullTile();

            if (existingTile != null)
            {
                tileToPaint = existingTile.CanvasTileData;
            }

            tileToPaint =
                tileToPaint.WithShapeData(TileType.Character, null, 0, character.ToString(), false, false, 0);

            _canvasSurface.Data.PlaceInkAt(targetPosition, tileToPaint, _canvasBrushFilter);
            xOffset++;
        }
    }

    private float CurrentTimer()
    {
        return Client.TotalElapsedTime - _lastTimeStamp;
    }

    private string BufferToString()
    {
        var builder = new StringBuilder();

        foreach (var character in _buffer)
        {
            builder.Append(character);
        }

        return builder.ToString();
    }
}
