using System.Diagnostics;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Gameplay;

public class FrameInput
{
    private static bool wasAnyDirectionTapped;
    private static Direction leftThumbstickLastHeldDirection = Direction.None;

    public static void UpdateInput(ConsumableInput input)
    {
        var newHeldDirection = CalculateHeldDirectionOnThumbstick(input);

        wasAnyDirectionTapped = input.Keyboard.GetButton(Keys.Left).WasPressed
                                || input.Keyboard.GetButton(Keys.Right).WasPressed
                                || input.Keyboard.GetButton(Keys.Down).WasPressed
                                || input.Keyboard.GetButton(Keys.Up).WasPressed

                                || input.Keyboard.GetButton(Keys.W).WasPressed
                                || input.Keyboard.GetButton(Keys.A).WasPressed
                                || input.Keyboard.GetButton(Keys.S).WasPressed
                                || input.Keyboard.GetButton(Keys.D).WasPressed

                                || input.GamePad.GetButton(Buttons.DPadLeft, PlayerIndex.One).WasPressed
                                || input.GamePad.GetButton(Buttons.DPadRight, PlayerIndex.One).WasPressed
                                || input.GamePad.GetButton(Buttons.DPadDown, PlayerIndex.One).WasPressed
                                || input.GamePad.GetButton(Buttons.DPadUp, PlayerIndex.One).WasPressed 
                                
                                || leftThumbstickLastHeldDirection != newHeldDirection;

        leftThumbstickLastHeldDirection = newHeldDirection;
    }
    
    public static bool AnyDirectionTapped()
    {
        return wasAnyDirectionTapped;
    }

    public static Direction HeldDirection(ConsumableInput input)
    {
        var direction = Direction.None;
        if (input.Keyboard.GetButton(Keys.Left).IsDown || input.Keyboard.GetButton(Keys.A).IsDown || input.GamePad.GetButton(Buttons.DPadLeft, PlayerIndex.One).IsDown)
        {
            direction = Direction.Left;
        }

        if (input.Keyboard.GetButton(Keys.Right).IsDown || input.Keyboard.GetButton(Keys.D).IsDown || input.GamePad.GetButton(Buttons.DPadRight, PlayerIndex.One).IsDown)
        {
            direction = Direction.Right;
        }

        if (input.Keyboard.GetButton(Keys.Up).IsDown || input.Keyboard.GetButton(Keys.W).IsDown || input.GamePad.GetButton(Buttons.DPadUp, PlayerIndex.One).IsDown)
        {
            direction = Direction.Up;
        }

        if (input.Keyboard.GetButton(Keys.Down).IsDown || input.Keyboard.GetButton(Keys.S).IsDown  || input.GamePad.GetButton(Buttons.DPadDown, PlayerIndex.One).IsDown)
        {
            direction = Direction.Down;
        }

        var thumbstickDirection = CalculateHeldDirectionOnThumbstick(input);

        if (thumbstickDirection != Direction.None)
        {
            return thumbstickDirection;
        }

        return direction;
    }

    private static Direction CalculateHeldDirectionOnThumbstick(ConsumableInput input)
    {
        var leftThumbstickVector = input.GamePad.LeftThumbstickPosition(PlayerIndex.One);
        // thumbstick Y Up is positive, but world Y Up is negative
        leftThumbstickVector.Y = -leftThumbstickVector.Y; 
        if (leftThumbstickVector.Length() > 0.25f)
        {
            return Direction.EstimateFromVector(leftThumbstickVector);
        }
        
        return Direction.None;
    }

    public static bool AnyActionTapped(ConsumableInput input)
    {
        return PrimaryActionTapped(input) || SecondaryActionTapped(input);
    }

    public static bool PrimaryActionTapped(ConsumableInput input)
    {
        return input.Keyboard.GetButton(Keys.Z).WasPressed || input.Keyboard.GetButton(Keys.Space).WasPressed || input.Keyboard.GetButton(Keys.Enter).WasPressed || input.GamePad.GetButton(Buttons.A,PlayerIndex.One).WasPressed || input.GamePad.GetButton(Buttons.Y,PlayerIndex.One).WasPressed;
    }

    public static bool SecondaryActionTapped(ConsumableInput input)
    {
        return input.Keyboard.GetButton(Keys.X).WasPressed || input.Keyboard.GetButton(Keys.LeftShift).WasPressed || input.Keyboard.GetButton(Keys.RightShift).WasPressed || input.GamePad.GetButton(Buttons.X,PlayerIndex.One).WasPressed || input.GamePad.GetButton(Buttons.B,PlayerIndex.One).WasPressed;
    }

    public static bool CancelPressed(ConsumableInput input)
    {
        return input.Keyboard.GetButton(Keys.Escape).WasPressed;
    }

    public static bool ResetPressed(ConsumableInput input)
    {
        return input.Keyboard.GetButton(Keys.R).WasPressed || input.Keyboard.GetButton(Keys.Escape).WasPressed || input.GamePad.GetButton(Buttons.Start,PlayerIndex.One).WasPressed || input.GamePad.GetButton(Buttons.Back,PlayerIndex.One).WasPressed;
    }
}
