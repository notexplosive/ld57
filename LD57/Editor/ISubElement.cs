using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public interface ISubElement
{
    void PutSubElementOnScreen(AsciiScreen screen, bool isHovered);
    bool Contains(GridPosition relativePosition);
    void OnClicked();
    void OnTextInput(char[] enteredCharacters);
    void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard);
}
