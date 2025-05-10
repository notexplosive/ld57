using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public class TriggerTool : IEditorTool
{
    private readonly EditorSession _editorSession;
    private readonly WorldEditorSurface _surface;

    public TriggerTool(EditorSession editorSession, WorldEditorSurface surface)
    {
        _editorSession = editorSession;
        _surface = surface;
    }

    public TileState TileStateInToolbar { get; } = TileState.StringCharacter("!");

    public string Status()
    {
        if (_editorSession.HoveredWorldPosition.HasValue)
        {
            var metadataEntities = _surface.Data
                .GetMetadataAt(_editorSession.HoveredWorldPosition.Value).ToList();

            if (metadataEntities.Count > 0 &&
                metadataEntities.First().ExtraState.TryGetValue(Constants.CommandKey, out var status))
            {
                return $"{Constants.CommandKey}: " + status;
            }
        }

        return "";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard, GridPosition? hoveredWorldPosition)
    {
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            var foundMetaEntity = _surface.Data.GetMetadataAt(position).FirstOrDefault();
            var defaultText = "";
            if (foundMetaEntity != null)
            {
                defaultText = foundMetaEntity.ExtraState.GetValueOrDefault(Constants.CommandKey) ?? defaultText;
            }

            var isUsingSelection = _surface.Selection.Contains(position);
            _editorSession.RequestText("Enter Command", defaultText,
                text =>
                {
                    if (foundMetaEntity != null)
                    {
                        if (string.IsNullOrEmpty(text))
                        {
                            _surface.Data.RemoveExact(foundMetaEntity);
                        }
                        else
                        {
                            foundMetaEntity.ExtraState[Constants.CommandKey] = text;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(text))
                        {
                            if (isUsingSelection)
                            {
                                foreach (var cell in _surface.Selection.AllPositions())
                                {
                                    _surface.Data.AddMetaEntity(cell, text);
                                }
                            }
                            else
                            {
                                _surface.Data.AddMetaEntity(position, text);
                            }
                        }
                    }
                });
        }
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
    }

    public void PaintToWorld(AsciiScreen screen)
    {
    }

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        return TileState.StringCharacter("!");
    }
}
