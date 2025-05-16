using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class KeybindChord
{
    private readonly Keys _firstKey;
    private readonly List<SecondNote> _secondNotes = new();
    private readonly List<Func<GridPosition,ISubElement>> _subElements = new();
    private readonly string _title;

    public KeybindChord(Keys firstKey, string title)
    {
        _firstKey = firstKey;
        _title = title;
    }

    public void ListenForFirstKey(ConsumableInput input, IEditorSurface surface)
    {
        if (input.Keyboard.GetButton(_firstKey, true).WasPressed)
        {
            surface.RequestPopup(screenSize =>
            {
                var rectangle = GridRectangle.FromCenterAndSize(screenSize.Center, PopupSize());
                var chordPopup = new ChordPopup(rectangle, this, Title(), _subElements, new GridPosition());
                return chordPopup;
            });
        }
    }

    private string Title()
    {
        return _title;
    }

    private GridPosition PopupSize()
    {
        var width = Title().Length;

        foreach (var note in _secondNotes)
        {
            width = Math.Max(width, note.Description.Length + 5);
        }

        var height = _secondNotes.Count;

        if (_subElements.Count > 0)
        {
            height++;
        }

        return new GridPosition(width, height) + new GridPosition(1, 2);
    }
    
    public (ChordNoteStatus, SecondNote?) ListenForSecondNote(ConsumableInput.ConsumableKeyboard keyboard)
    {
        foreach (var note in _secondNotes)
        {
            var state = keyboard.GetButton(note.Key);
            if (state.WasPressed)
            {
                keyboard.Consume(note.Key);
                return (ChordNoteStatus.Pressed, note);
            }
            
            if (state.WasReleased)
            {
                keyboard.Consume(note.Key);
                return (ChordNoteStatus.Released, note);
            }
        }

        return (ChordNoteStatus.None, null);
    }

    public KeybindChord Add(Keys key, string label, bool shouldCloseOnPress, Action function)
    {
        _secondNotes.Add(new SecondNote(key, label, function, shouldCloseOnPress));
        return this;
    }

    public IEnumerable<SecondNote> SecondNotes()
    {
        return _secondNotes;
    }

    public KeybindChord AddDynamicTile(Func<TileState> func)
    {
        _subElements.Add(position => new DynamicTile(position, func));
        return this;
    }

    public record SecondNote(Keys Key, string Description, Action Function, bool ShouldCloseImmediately);
}

public enum ChordNoteStatus
{
    None,
    Pressed,
    Released
}