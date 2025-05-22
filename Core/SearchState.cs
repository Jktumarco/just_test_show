public class SearchState : BaseState
{
    public SearchState(StateMachine stateMachine, StateHelper stateHelper)
        : base(stateMachine, stateHelper) { }

    public override void Enter()
    {
        stateHelper.SetAnimation("idle");
    }

    public override void Update()
    {
        if (stateHelper.TryFindTarget() && stateHelper.HasValidTarget())
        {
            stateMachine.ChangeState(new AttackState(stateMachine, stateHelper));
        }
        else
        {
            stateHelper.SetAnimation("idle");
        }
    }

    public override void Exit() { }
}
