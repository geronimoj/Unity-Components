using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class If_ElseTransition : Transition
{
    public State ifState;
    public State elseState;
    public override bool ShouldTransition(ref PlayerController ctrl)
    {
        if (Condition(ref ctrl))
            targetState = ifState;
        else
            targetState = elseState;

        return true;
    }

    protected virtual bool Condition(ref PlayerController ctrl)
    {
        return false;
    }
}
