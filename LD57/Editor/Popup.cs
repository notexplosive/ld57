using System;
using LD57.Rendering;

namespace LD57.Editor;

public class Popup : UiElement
{
    public Popup(GridRectangle corners) : base(corners)
    {
    }

    public void Close()
    {
        ShouldClose = true;
    }

    public bool ShouldClose { get; private set; }
}
