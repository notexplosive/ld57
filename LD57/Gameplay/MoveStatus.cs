using System;
using System.Collections.Generic;

namespace LD57.Gameplay;

public record struct MoveStatus()
{
    private int _evaluationCount;
    public bool ShouldEvaluate { get; private set; } = true;
    public bool WasSuccessful { get; private set; } = true;
    public bool CausedPush { get; private set; } = false;

    public void StartEvaluation()
    {
        ShouldEvaluate = false;
    }
    
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
        else
        {
            ShouldEvaluate = true;
            
            _evaluationCount++;
            if (_evaluationCount > 100)
            {
                throw new Exception("Evaluation looped too many times");
            }
        }
    }
}
