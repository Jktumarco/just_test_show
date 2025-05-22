public class MoveState : BaseState
{
    public MoveState(StateMachine stateMachine, StateHelper stateHelper)
        : base(stateMachine, stateHelper) { }

    public override void Enter()
    {
        stateHelper.ClearTarget();
    }

    public override void Update()
    {
        if (!stateHelper.IsMoving())
        {
            stateHelper.SetAnimation("idle");
        }
    }

    public override void Exit() { }
}
