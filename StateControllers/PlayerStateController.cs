using UniRx;

public class PlayerStateController : IStateController, System.IDisposable
{
    private StateHelper _stateHelper;
    private IInputSystem _input;
    private Detector _detector;
    private StateMachine _stateMachine;
    private CompositeDisposable _disposables;

    public PlayerStateController(BaseCharacter character, ReactVariable<CharacterConfig> playerConfig,
        IAnimatorController animatorController, Detector enemyDetector, IMove movement, ReactVariable<IWeapon> weapon, IInputSystem input)
    {
        _input = input;
        _detector = enemyDetector;
        _disposables = new CompositeDisposable();
        _stateMachine = new StateMachine();

        _stateHelper = new StateHelper(character, _stateMachine, playerConfig, animatorController, weapon, movement, this, enemyDetector, null, input);
        _stateMachine.Init(new SearchState(_stateMachine, _stateHelper));

        SubscribeToInput();
    }
    public void Update()
    {
        _stateHelper.UpdateTimers();
        _stateMachine.Update();
        _stateHelper.Move();
    }

    private void SubscribeToInput()
    {
        _input.GetMagnitude().Subscribe(isMoving =>
        {
            if (isMoving)
            {
                EventSystem.OnMoveRotateWithJoy.Value = isMoving;
                if (!(_stateMachine.CurrentState is MoveState))
                {
                    _stateMachine.ChangeState(new MoveState(_stateMachine, _stateHelper));
                }
            }
            else
            {
                if (_detector.IsDetectEnemy())
                {
                    if (!(_stateMachine.CurrentState is AttackState))
                    {
                        if (_stateHelper.TryFindTarget() && _stateHelper.HasValidTarget())
                        {
                            _stateMachine.ChangeState(new AttackState(_stateMachine, _stateHelper));
                        }
                        else _stateMachine.ChangeState(new SearchState(_stateMachine, _stateHelper));
                    }
                }
                else
                {
                    if (!(_stateMachine.CurrentState is SearchState))
                    {
                        _stateMachine.ChangeState(new SearchState(_stateMachine, _stateHelper));
                    }
                }
            }
        }).AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
