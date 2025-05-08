using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public interface ISubElement
{
    void PutOnScreen(AsciiScreen screen, GridPosition topLeft);
    bool Contains(GridPosition relativePosition);
    void ShowHover(AsciiScreen screen, GridPosition hoveredTilePosition);
    void OnClicked();
    void OnTextInput(char[] enteredCharacters);
    void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard);
}
