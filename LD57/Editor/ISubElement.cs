using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public interface ISubElement
{
    void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement);
    bool Contains(GridPosition relativePosition);
    void OnClicked();
    void OnTextInput(char[] enteredCharacters);
    void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard);
    void OnScroll(int scrollDelta, ISubElement? hoveredElement, ModifierKeys keyboardModifiers);
}