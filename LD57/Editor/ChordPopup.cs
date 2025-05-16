using System;
using System.Collections.Generic;
using System.Linq;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class ChordPopup : Popup
{
    public ChordPopup(GridRectangle screenSize, KeybindChord keybindChord, string title, List<Func<GridPosition,ISubElement>> subElements,
        GridPosition startPosition) : base(screenSize)
    {
        AddInputListener(keyboard =>
        {
            var (status, note) = keybindChord.ListenForSecondNote(keyboard);

            if (note != null)
            {
                if (status == ChordNoteStatus.Pressed)
                {
                    note.Function();
                    if (note.ShouldCloseImmediately)
                    {
                        Close();
                    }
                }
                
                if (status == ChordNoteStatus.Released)
                {
                    Close();
                }
            }
            else
            {
                if (keyboard.GetButton(Keys.Escape).WasPressed)
                {
                    Close();
                }
            }
        });
        
        AddStaticText(new GridPosition(1, 1), title, Color.Yellow);
        var x = 0;
        var y = 1;

        if (subElements.Count > 0)
        {
            y++;
        }
        
        foreach (var subElementFactory in subElements)
        {
            AddSubElement(subElementFactory(new GridPosition(x, 1) + new GridPosition(1,1)));
            x++;
        }
        
        foreach (var secondNote in keybindChord.SecondNotes())
        {
            AddStaticText(new GridPosition(1, 1 + y) + startPosition, $"[{secondNote.Key}]: {secondNote.Description}");
            y++;
        }
    }
}
