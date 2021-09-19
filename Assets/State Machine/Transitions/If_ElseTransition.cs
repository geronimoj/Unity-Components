using StateMachine.States;

namespace StateMachine.Transitions
{
    public class If_ElseTransition<T> : Transition<T>
    {
        public State<T> ifState;
        public State<T> elseState;
        public override bool ShouldTransition(ref T ctrl)
        {
            if (Condition(ref ctrl))
                targetState = ifState;
            else
                targetState = elseState;

            return true;
        }

        protected virtual bool Condition(ref T ctrl)
        {
            return false;
        }
    }
}