using LD57.Rendering;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class ChooseTileModal : Popup
{
    public ChooseTileModal(GridRectangle corners) : base(corners)
    {
        AddInputListener(keyboard =>
        {
            if (keyboard.GetButton(Keys.Escape).WasPressed)
            {
                Close();
            }
        });
    }
}
