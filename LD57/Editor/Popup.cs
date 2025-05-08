using System;
using LD57.Rendering;

namespace LD57.Editor;

public class Popup : UiElement
{
    public Popup(GridRectangle corners) : base(corners)
    {
    }

    public event Action? RequestClosePopup;

    public void Close()
    {
        RequestClosePopup?.Invoke();
    }
}
