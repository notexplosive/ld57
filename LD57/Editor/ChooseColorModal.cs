using System;
using ExplogineCore.Data;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class ChooseColorModal : Popup
{
    public ChooseColorModal(GridRectangle rectangle, Func<string> getColor, Func<float>? getIntensity = null) :
        base(rectangle)
    {
        AddInputListener(keyboard =>
        {
            if (keyboard.GetButton(Keys.Escape).WasPressed)
            {
                Close();
            }
        });

        var topLeftPadding = new GridPosition(1, 1);

        if (getIntensity != null)
        {
            var numberOfIntensities = 10;
            for (var intensityIndex = 0; intensityIndex <= numberOfIntensities; intensityIndex++)
            {
                var intensity = (float) intensityIndex / 10;

                var buttonPosition = topLeftPadding + new GridPosition(intensityIndex, 0);

                var intensityButton = new Button(buttonPosition, () => { ChooseIntensity(intensity); });

                intensityButton.SetTileStateGetter(() =>
                {
                    var isSelected = Math.Abs(getIntensity() - intensity) < 0.01f;
                    
                    if (intensity == 0)
                    {
                        if (isSelected)
                        {
                            return TileState.Sprite(ResourceAlias.Utility, 0, Color.Yellow);
                        }

                        return TileState.Sprite(ResourceAlias.Utility, 0, Color.LightBlue);
                    }

                    if (isSelected)
                    {
                        return TileState.BackgroundOnly(Color.Yellow, intensity);
                    }

                    return TileState.BackgroundOnly(Color.LightBlue, intensity);
                });
                AddButton(intensityButton);
            }

            topLeftPadding += new GridPosition(0, 1);
        }
        
        
        AddExtraDraw(new ExtraDraw(DrawExtraBorder));
        
        var scrollablePane = AddScrollablePane(new GridRectangle(topLeftPadding, new GridPosition(rectangle.Width,rectangle.Height) - new GridPosition(1,1)));
        
        var i = 0;
        var selectedIndex = 0;
        foreach (var (key, color) in LdResourceAssets.Instance.NamedColors)
        {
            if (!color.HasValue)
            {
                continue;
            }

            var buttonPosition = new GridPosition(0, i);
            var colorButton = new Button(buttonPosition, () =>
            {
                ChooseColor(key);
                Close();
            });

            if (getColor() == key)
            {
                selectedIndex = i;
            }
            
            colorButton.SetTileStateGetter(() =>
            {
                if (getColor() == key)
                {
                    return TileState.Sprite(ResourceAlias.Entities, 0).WithBackground(color.Value);
                }

                return TileState.BackgroundOnly(color.Value, 1);
            });
            
            scrollablePane.AddButton(colorButton);
            scrollablePane.AddStaticText(buttonPosition + new GridPosition(1, 0), key);
            i++;
        }
        
        var scrollBarRectangle = new GridRectangle(
            scrollablePane.ViewPort.TopRight + new GridPosition(0, 0),
            scrollablePane.ViewPort.BottomRight + new GridPosition(0, 0));
        var dynamicImage = AddDynamicImage(scrollBarRectangle);
        dynamicImage.SetDrawAction(screen =>
        {
            if (scrollablePane.ShouldHaveThumb())
            {
                var thumbPosition = scrollablePane.ThumbPosition(scrollablePane.ViewRectangle.Height);
                screen.PutFilledRectangle(TileState.Sprite(ResourceAlias.Tools, 17, Color.Gray),
                    scrollBarRectangle.MovedToZero());
                screen.PutTile(new GridPosition(0, thumbPosition),
                    TileState.Sprite(ResourceAlias.Tools, 16, Color.LightBlue));
            }
            else
            {
                screen.PutFilledRectangle(TileState.Sprite(ResourceAlias.Tools, 17, ResourceAlias.Color("background")),
                    scrollBarRectangle.MovedToZero());
            }
        });
        
        scrollablePane.SetContentHeight(i);
        scrollablePane.ScrollToPosition(selectedIndex);
    }

    private void DrawExtraBorder(AsciiScreen screen)
    {
        var width = Rectangle.Width;
        screen.PutTile(new GridPosition(width, 0), TileState.Sprite(ResourceAlias.PopupFrame, 1));
        screen.PutTile(new GridPosition(width, 1), TileState.TransparentEmpty);
        screen.PutTile(new GridPosition(width, 2),
            TileState.Sprite(ResourceAlias.Utility, 27).WithFlip(new XyBool(true, false)));
        screen.PutTile(new GridPosition(width + 1, 2), TileState.Sprite(ResourceAlias.PopupFrame, 5));
        screen.PutTile(new GridPosition(width + 1, 0), TileState.Sprite(ResourceAlias.PopupFrame, 1));
        screen.PutTile(new GridPosition(width + 2, 0), TileState.Sprite(ResourceAlias.PopupFrame, 2));
        screen.PutTile(new GridPosition(width + 2, 1), TileState.Sprite(ResourceAlias.PopupFrame, 3));
        screen.PutTile(new GridPosition(width + 2, 2), TileState.Sprite(ResourceAlias.PopupFrame, 4));
    }

    private void ChooseColor(string color)
    {
        ChoseColor?.Invoke(color);
    }

    private void ChooseIntensity(float intensity)
    {
        ChoseIntensity?.Invoke(intensity);
    }

    public event Action<string>? ChoseColor;
    public event Action<float>? ChoseIntensity;
}
