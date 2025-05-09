using System;
using System.Linq;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class ChooseColorModal : Popup
{
    private readonly bool _shouldUseIntensity;

    public ChooseColorModal(GridRectangle rectangle, Func<string> getColor, Func<float>? getIntensity = null) : base(rectangle)
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
            _shouldUseIntensity = true;

            var numberOfIntensities = 10;
            for (int intensityIndex = 0; intensityIndex < numberOfIntensities; intensityIndex++)
            {
                var intensity = (float)intensityIndex / 10;
                
                var buttonPosition = topLeftPadding + new GridPosition(intensityIndex, 0);

                var intensityButton = new Button(buttonPosition, () =>
                {
                    ChooseIntensity(intensity);
                });
                
                intensityButton.SetTileStateGetter(() =>
                {
                    if (Math.Abs(getIntensity() - intensity) < 0.01f)
                    {
                        return TileState.BackgroundOnly(Color.Yellow, intensity);
                    }
                    return TileState.BackgroundOnly(Color.LightBlue, intensity);
                });
                AddButton(intensityButton);
            }
            
            topLeftPadding += new GridPosition(0, 1);
        }

        var i = 0;
        foreach (var (key, color) in LdResourceAssets.Instance.NamedColors)
        {
            if (!color.HasValue)
            {
                continue;
            }

            var buttonPosition = topLeftPadding + new GridPosition(0,i);
            var colorButton = new Button(buttonPosition, () =>
            {
                ChooseColor(key);
                Close();
            });
            colorButton.SetTileStateGetter(() =>
            {
                if (getColor() == key)
                {
                    return TileState.Sprite(ResourceAlias.Entities, 0).WithBackground(color.Value, 1); 
                }

                return TileState.BackgroundOnly(color.Value, 1);
            });
            AddButton(colorButton);
            
            AddStaticText(buttonPosition + new GridPosition(1,0), key);
            i++;
        }
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
