using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

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
        if (_editorSession.IsDraggingSecondary)
        {
            return TileState.TransparentEmpty;
        }

        return _editorSession.SelectedTemplate?.CreateAppearance().TileState ?? TileState.TransparentEmpty;
    }

    public string Status()
    {
        return "Brush";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        if (!_editorSession.HoveredWorldPosition.HasValue)
        {
            return;
        }

        if (_editorSession.SelectedTemplate == null)
        {
            return;
        }

        if (_editorSession.IsDraggingPrimary)
        {
            _editorSession.Surface.WorldTemplate.SetTile(_editorSession.HoveredWorldPosition.Value, _editorSession.SelectedTemplate);
        }

        if (_editorSession.IsDraggingSecondary)
        {
            _editorSession.Surface.WorldTemplate.RemoveEntitiesAtExceptMetadata(_editorSession.HoveredWorldPosition.Value);
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
