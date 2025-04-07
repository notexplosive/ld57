using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework.Input;

namespace LD57.Gameplay;

public class FrameInput
{
    public bool AnyDirectionTapped(ConsumableInput input)
    {
        return
            input.Keyboard.GetButton(Keys.Left).WasPressed
            || input.Keyboard.GetButton(Keys.Right).WasPressed
            || input.Keyboard.GetButton(Keys.Down).WasPressed
            || input.Keyboard.GetButton(Keys.Up).WasPressed
            
            || input.Keyboard.GetButton(Keys.W).WasPressed
            || input.Keyboard.GetButton(Keys.A).WasPressed
            || input.Keyboard.GetButton(Keys.S).WasPressed
            || input.Keyboard.GetButton(Keys.D).WasPressed
            ;
    }

    public Direction HeldDirection(ConsumableInput input)
    {
        var direction = Direction.None;
        if (input.Keyboard.GetButton(Keys.Left).IsDown || input.Keyboard.GetButton(Keys.A).IsDown)
        {
            direction = Direction.Left;
        }

        if (input.Keyboard.GetButton(Keys.Right).IsDown || input.Keyboard.GetButton(Keys.D).IsDown)
        {
            direction = Direction.Right;
        }

        if (input.Keyboard.GetButton(Keys.Up).IsDown || input.Keyboard.GetButton(Keys.W).IsDown)
        {
            direction = Direction.Up;
        }

        if (input.Keyboard.GetButton(Keys.Down).IsDown || input.Keyboard.GetButton(Keys.S).IsDown)
        {
            direction = Direction.Down;
        }

        return direction;
    }

    public static bool AnyActionTapped(ConsumableInput input)
    {
        return PrimaryActionTapped(input) || SecondaryActionTapped(input);
    }

    public static bool PrimaryActionTapped(ConsumableInput input)
    {
        return input.Keyboard.GetButton(Keys.Z).WasPressed || input.Keyboard.GetButton(Keys.Space).WasPressed || input.Keyboard.GetButton(Keys.Enter).WasPressed;
    }

    public static bool SecondaryActionTapped(ConsumableInput input)
    {
        return input.Keyboard.GetButton(Keys.X).WasPressed || input.Keyboard.GetButton(Keys.LeftShift).WasPressed || input.Keyboard.GetButton(Keys.RightShift).WasPressed;
    }
}
