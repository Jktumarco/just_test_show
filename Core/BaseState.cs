public abstract class BaseState
{
    protected StateMachine stateMachine;
    protected StateHelper stateHelper;

    public BaseState(StateMachine stateMachine, StateHelper stateHelper)
    {
        this.stateMachine = stateMachine;
        this.stateHelper = stateHelper;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
