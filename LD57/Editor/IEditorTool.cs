using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public interface IEditorTool
{
    TileState TileStateInToolbar { get; }
    TileState GetTileStateInWorldOnHover(TileState original);
    string Status();
    void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard, GridPosition? hoveredWorldPosition);
    void StartMousePressInWorld(GridPosition position, MouseButton mouseButton);
    void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton);
    void PaintToWorld(AsciiScreen screen);
    
}
