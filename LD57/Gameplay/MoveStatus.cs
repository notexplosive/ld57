using System;
using System.Collections.Generic;

namespace LD57.Gameplay;

public record struct MoveStatus()
{
    private readonly List<Action> _actions = new();

    public bool WasSuccessful { get; private set; } = true;
    public bool CausedPush { get; private set; } = false;

    public void Fail()
    {
        WasSuccessful = false;
    }

    public void DependOnMove(MoveStatus cascadedMove)
    {
        CausedPush = true;
        if (!cascadedMove.WasSuccessful)
        {
            Fail();
        }
    }

    public void AddAction(Action action)
    {
        _actions.Add(action);
    }

    public void ExecuteActions()
    {
        foreach (var action in _actions)
        {
            action();
        }

        _actions.Clear();
    }
}
