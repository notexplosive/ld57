using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class PromptOption
{
    private readonly Action _onChoose;

    public PromptOption(string text, Action onChoose)
    {
        var color = Color.White;
        Text = MessagePage.BuildLine(text, ref color);
        _onChoose = onChoose;
    }

    public List<TileState> Text { get; }

    public void Choose()
    {
        _onChoose();
    }

    public int Width()
    {
        return Text.Count;
    }
}

public class Prompt
{
    public Prompt(string title, Orientation orientation, List<PromptOption> options)
    {
        Orientation = orientation;

        var color = ResourceAlias.Color("blue_text");
        Title = MessagePage.BuildLine(title, ref color);
        Options = options;
    }

    public Orientation Orientation { get; }
    public List<TileState> Title { get; }
    public List<PromptOption> Options { get; }

    public int Width()
    {
        if (Orientation == Orientation.Vertical)
        {
            var width = Title.Count;
            foreach (var option in Options)
            {
                width = Math.Max(width, option.Width());
            }

            return width + 1;
        }
        else
        {
            var padding = 1;
            var width = 0;
            foreach (var option in Options)
            {
                width += option.Width();
                width += padding;
            }
            
            return width + 1;
        }
    }

    public float Height()
    {
        if (Orientation == Orientation.Vertical)
        {
            // each option plus title plus spacer, plus 1 more for good luck
            return Options.Count + 3;
        }

        // title + options row + spacer
        return 4;
    }
}
