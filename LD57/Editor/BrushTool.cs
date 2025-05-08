using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public abstract class BrushTool : IEditorTool
{
    protected BrushTool(EditorSession editorEditorSession)
    {
        EditorSession = editorEditorSession;
    }

    protected EditorSession EditorSession { get; }

    public TileState TileStateInToolbar => TileState.Sprite(ResourceAlias.Tools, 0);

    public abstract TileState GetTileStateInWorldOnHover(TileState original);

    public string Status()
    {
        return "Brush";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard, GridPosition? hoveredWorldPosition)
    {
        if (hoveredWorldPosition.HasValue)
        {
            if (EditorSession.IsDraggingPrimary)
            {
                OnPaint(hoveredWorldPosition.Value);
            }

            if (EditorSession.IsDraggingSecondary)
            {
                OnErase(hoveredWorldPosition.Value);
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

    public void PaintToWorld(AsciiScreen screen, GridPosition cameraPosition)
    {
        // do nothing
    }

    protected abstract void OnErase(GridPosition hoveredWorldPosition);

    protected abstract void OnPaint(GridPosition hoveredWorldPosition);
}
