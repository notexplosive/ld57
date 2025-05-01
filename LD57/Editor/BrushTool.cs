using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Core;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class BrushTool : IEditorTool
{
    private readonly EditorSession _editorSession;

    public BrushTool(EditorSession editorSession)
    {
        _editorSession = editorSession;
    }

    public TileState TileStateInToolbar => TileState.Sprite(ResourceAlias.Tools, 0);
    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        // todo: this should show the tile we're about to place
        return original with {BackgroundColor = Color.LightBlue, BackgroundIntensity = 0.75f};
    }

    public string Status()
    {
        return "Brush";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        if (_editorSession.HoveredWorldPosition.HasValue && _editorSession.SelectedTemplate != null)
        {
            if (_editorSession.IsDraggingPrimary)
            {
                _editorSession.WorldTemplate.SetTile(_editorSession.HoveredWorldPosition.Value, _editorSession.SelectedTemplate);
            }

            if (_editorSession.IsDraggingSecondary)
            {
                _editorSession.WorldTemplate.RemoveEntitiesAtExceptMetadata(_editorSession.HoveredWorldPosition.Value);
            }
        }
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        // do nothing
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        // do nothing
    }

    public void PaintToScreen(AsciiScreen screen, GridPosition cameraPosition)
    {
        
    }
}