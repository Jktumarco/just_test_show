using UniRx;

public class EnemyStateController : IStateController, System.IDisposable
{
    private StateMachine _stateMachine;
    private StateHelper _stateHelper;
    private BaseCharacterPrefabConfig _prefabConfig;  
    private bool _can = false;

    private CompositeDisposable _disposables = new CompositeDisposable();

    public EnemyStateController(BaseCharacter character, ReactVariable<CharacterConfig> characterConfig,IAnimatorController animatorController,IMove movement,ReactVariable<IWeapon> weapon,Detector enemyDetector, BaseCharacterPrefabConfig prefabConfig)
    {
        _prefabConfig = prefabConfig;
        _stateMachine = new StateMachine();
        _stateHelper = new StateHelper(character,_stateMachine,characterConfig,animatorController,weapon, movement,this, enemyDetector, _prefabConfig
        );

        _stateMachine.Init(
            new PatrolState(_stateMachine, _stateHelper)
        );

        EventSystem.OnPlayerLanding
            .Subscribe(connectionData =>
            {
                if (connectionData.Success &&
                    _stateHelper.Character.transform.parent
                        .Find(connectionData.LandedIsland.name) != null)
                {
                    _can = true;
                }
                else
                {
                    _can = false;
                    if (!(_stateMachine.CurrentState is PatrolState))
                        _stateMachine.ChangeState(
                            new PatrolState(_stateMachine, _stateHelper)
                        );
                }
            }).AddTo(_disposables);
    }

    public void Update()
    {
        _stateHelper.UpdateTimers();
        _stateMachine.Update();

        if (!_can) return;

        if (_stateHelper.TryFindTarget())
        {
            if (_stateHelper.HasValidTarget())
            {
                if (!(_stateMachine.CurrentState is BanditsAI.BanditsAttackState))
                    _stateMachine.ChangeState(
                        new BanditsAI.BanditsAttackState(_stateMachine, _stateHelper)
                    );
            }
            else
            {
                if (!(_stateMachine.CurrentState is BanditsAI.BanditsFollowState))
                    _stateMachine.ChangeState(
                        new BanditsAI.BanditsFollowState(_stateMachine, _stateHelper)
                    );
            }
        }
        else
        {
            if (!(_stateMachine.CurrentState is PatrolState))
                _stateMachine.ChangeState(
                    new PatrolState(_stateMachine, _stateHelper)
                );
        }
    }
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
