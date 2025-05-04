using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Editor;

public class TriggerTool : IEditorTool
{
    private readonly EditorSession _editorSession;

    public TriggerTool(EditorSession editorSession)
    {
        _editorSession = editorSession;
    }

    public TileState TileStateInToolbar { get; } = TileState.StringCharacter("!");

    public string Status()
    {
        if (_editorSession.HoveredWorldPosition.HasValue)
        {
            var metadataEntities = _editorSession.Surface.WorldTemplate.GetMetadataAt(_editorSession.HoveredWorldPosition.Value).ToList();

            if (metadataEntities.Count > 0 &&
                metadataEntities.First().ExtraState.TryGetValue(Constants.CommandKey, out var status))
            {
                return $"{Constants.CommandKey}: " + status;
            }
        }

        return "";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            var foundMetaEntity = _editorSession.Surface.WorldTemplate.GetMetadataAt(position).FirstOrDefault();
            var defaultText = "";
            if (foundMetaEntity != null)
            {
                defaultText = foundMetaEntity.ExtraState.GetValueOrDefault(Constants.CommandKey) ?? defaultText;
            }

            var isUsingSelection = _editorSession.WorldSelection.Contains(position);
            _editorSession.RequestText("Enter Command", defaultText,
                text =>
                {
                    if (foundMetaEntity != null)
                    {
                        if (string.IsNullOrEmpty(text))
                        {
                            _editorSession.Surface.WorldTemplate.RemoveExactEntity(foundMetaEntity);
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
                                foreach (var cell in _editorSession.WorldSelection.AllPositions())
                                {
                                    _editorSession.Surface.WorldTemplate.AddMetaEntity(cell, text);
                                }
                            }
                            else
                            {
                                _editorSession.Surface.WorldTemplate.AddMetaEntity(position, text);
                            }
                        }
                    }
                });
        }
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
    }

    public void PaintToScreen(AsciiScreen screen, GridPosition cameraPosition)
    {
        
    }

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        return TileState.StringCharacter("!");
    }
}
