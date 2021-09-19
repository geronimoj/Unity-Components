/// <summary>
/// A transition for a If_ElseIf transition requirements
/// </summary>
public class If_ElseIfTransition : Transition
{
    /// <summary>
    /// The state to swap to if the if passes
    /// </summary>
    public State ifState;
    /// <summary>
    /// The state to swap to if the elseIf passes
    /// </summary>
    public State elseIfState;

    public override bool ShouldTransition(ref PlayerController ctrl)
    {   //Perform any universal checks that both IfCondition and elseIfCondition may want
        if (!UniversalChecks(ref ctrl))
            return false;
        //Check if condition
        if (IfCondition(ref ctrl))
        {
            targetState = ifState;
            return true;
        }
        //Check elseIf condition
        else if (IfElseCondition(ref ctrl))
        {
            targetState = elseIfState;
            return true;
        }
        //Return false
        return false;
    }
    /// <summary>
    /// Perform any checks / changes necessary before the other conditions get called
    /// </summary>
    /// <param name="c">A reference to the player controller</param>
    /// <returns>Returns true if you want to do the conditional checks. Returning false does not change the state</returns>
    protected virtual bool UniversalChecks(ref PlayerController c) { return true; }
    /// <summary>
    /// The check for the ifState
    /// </summary>
    /// <param name="c">A reference to the player controller</param>
    /// <returns>Returns true if the condition is met</returns>
    protected virtual bool IfCondition(ref PlayerController c) { return false;  }

    /// <summary>
    /// The check for the elseIfState
    /// </summary>
    /// <param name="c">A reference to the player controller</param>
    /// <returns>Returns true if the condition is met</returns>
    protected virtual bool IfElseCondition(ref PlayerController c) { return false; }
}
