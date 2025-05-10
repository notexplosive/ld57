using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public class PlayTool : IEditorTool
{
    private readonly EditorSession _editorSession;
    private readonly WorldEditorSurface _surface;

    public PlayTool(EditorSession editorSession, WorldEditorSurface surface)
    {
        _editorSession = editorSession;
        _surface = surface;
    }

    public TileState TileStateInToolbar => TileState.Sprite(ResourceAlias.Tools, 6);

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        return TileState.Sprite(ResourceAlias.Entities, 0);
    }

    public string Status()
    {
        return "Play from Select Location";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard, GridPosition? hoveredWorldPosition)
    {
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left && position.HasValue)
        {
            if (_editorSession.Surface.FileName != null)
            {
                _editorSession.SaveFlow();
                _surface.RequestPlay(position.Value);
            }
            else
            {
                _editorSession.SaveFlow();
            }
        }
    }

    public void PaintToWorld(AsciiScreen screen)
    {
        
    }
}
