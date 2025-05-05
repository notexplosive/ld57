using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public abstract class BrushTool : IEditorTool
{
    protected EditorSession EditorSession { get; }

    protected BrushTool(EditorSession editorEditorSession)
    {
        EditorSession = editorEditorSession;
    }

    public TileState TileStateInToolbar => TileState.Sprite(ResourceAlias.Tools, 0);

    public abstract TileState GetTileStateInWorldOnHover(TileState original);

    public string Status()
    {
        return "Brush";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        if (EditorSession.IsDraggingPrimary)
        {
            OnPaint();
        }

        if (EditorSession.IsDraggingSecondary)
        {
            OnErase();
        }
    }

    protected abstract void OnErase();

    protected abstract void OnPaint();

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
}