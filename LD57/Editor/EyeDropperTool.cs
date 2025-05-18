using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public abstract class EyeDropperTool : IEditorTool
{
    private readonly EditorSession _editorSession;
    public TileState TileStateInToolbar { get; } = TileState.Sprite(ResourceAlias.Tools, 21);
    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        return original;
    }

    protected EyeDropperTool(EditorSession editorSession)
    {
        _editorSession = editorSession;
    }

    public string Status()
    {
        if (_editorSession.HoveredWorldPosition.HasValue)
        {
            return DescribeTileAt(_editorSession.HoveredWorldPosition.Value);
        }

        return "";
    }

    protected abstract string DescribeTileAt(GridPosition position);

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard, GridPosition? hoveredWorldPosition)
    {
        
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (position.HasValue && mouseButton == MouseButton.Left)
        {
            GrabTile(position.Value);
        }
    }

    public abstract void GrabTile(GridPosition position);

    public void PaintToWorld(AsciiScreen screen)
    {
        
    }
}
