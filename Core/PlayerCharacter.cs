using UnityEngine;
using UniRx;

public class PlayerCharacter : BaseCharacter, IDamageableTargetable, IAnimatableCharacter
{
    private BaseCharacterPrefabConfig _prefabConfig;

    private IInputSystem _input;
    private IMove _movement;
    private IStateController _stateController;
    private IInteractionEnvironment _interactionEnvironment;
    public  IAnimatorController AnimatorController;

    private HealthStatus _healthStatus;
    private bool _isDie;

    private CompositeDisposable _disposables = new CompositeDisposable();

    public override void Init()
    {
        base.Init();
        start();
    }

    public override void Update()
    {
        base.Update();
        _stateController?.Update();
        _prefabConfig.HealthBar.FollowCameraRotate();
        _interactionEnvironment?.UpdateInteractions(transform.position);
    }

    private void start()
    {
        LoadConfig();
        _prefabConfig = GetComponent<BaseCharacterPrefabConfig>();

        var rb = _prefabConfig.Rb;
        var animator = _prefabConfig.Animator;
        var animEventController = _prefabConfig.AnimEvent;
        var detector = _prefabConfig.Detector;
        var healthBar = _prefabConfig.HealthBar;
        var weapon = _prefabConfig.Weapon;

        _healthStatus = new HealthStatus();
        _healthStatus.Max.Value = CharacterConfig.data.Health.data;
        _healthStatus.Current.Value = CharacterConfig.data.Health.data;
        _healthStatus.IsDead.Value = false;

        weapon_react.data = weapon;
        weapon_react.data.SetHolder(this);

        AnimatorController = new AnimatorController(animator, animEventController, AnimatorControllerParameterType.Trigger,
            new AnimatorController.AnimConfig(CharacterConfig.data.CharacterType, weapon_react));
        AnimatorController.LoadDefault(AssetNames.MainCharacter);

        _input = GameLocator.Get<IInputSystem>();
        _movement = new MovementWithJoy(rb, _input, CharacterConfig.data.RunSpeed.data);
        _stateController = new PlayerStateController(this, CharacterConfig, AnimatorController, detector, _movement, weapon_react, _input);

        detector.Init(this);

        _interactionEnvironment = new PlayerEnvironmentInteractionsSystem();
        _interactionEnvironment.Initialize(GameLocator.Get<LevelSystem>().InteractObjects, this);

        healthBar.SetHealth(_healthStatus.Current.Value);
        healthBar.SetMaxHealth(_healthStatus.Max.Value);
        healthBar.gameObject.SetActive(false);

        EventSystem.OnIsEnemyIsland
            .Subscribe(isEnemyIsland =>
            {
                if (isEnemyIsland)
                {
                    GameLocator.Get<GameWorldSystem>().AddCharacterToList(this);
                }
            }).AddTo(_disposables);       
    }

    public override void MakeAsPlayer()
    {
        base.MakeAsPlayer();
        _input = GameLocator.Get<IInputSystem>();
        _movement = new MovementWithJoy(_prefabConfig.Rb, _input, CharacterConfig.data.RunSpeed.data);
        _stateController = new PlayerStateController(this, CharacterConfig,
            new AnimatorController(_prefabConfig.Animator, _prefabConfig.AnimEvent, AnimatorControllerParameterType.Trigger,
            new AnimatorController.AnimConfig(CharacterConfig.data.CharacterType, weapon_react)),
            _prefabConfig.Detector, _movement, weapon_react, _input);
        _interactionEnvironment = new PlayerEnvironmentInteractionsSystem();
    }

    public override void MakeAsBot()
    {
        GameLocator.Get<GameWorldSystem>().RemoveCharacterFromList(this);
        new AnimatorController(_prefabConfig.Animator, _prefabConfig.AnimEvent, AnimatorControllerParameterType.Trigger,
            new AnimatorController.AnimConfig(CharacterConfig.data.CharacterType, weapon_react));
        _stateController = null;
        _movement = null;
        _interactionEnvironment.Cleanup();
    }

    public void TakeDamage(float amount = 0, BaseCharacter damageVisitor = null)
    {
        if (_isDie) { return; }
        coroutines.Instance.Execute(Damage(amount, damageVisitor));
    }

    private System.Collections.IEnumerator Damage(float amount, BaseCharacter damageVisitor = null)
    {
        if (_healthStatus.TakeDamage(amount))
        {
            _isDie = true;
            GameLocator.Get<GameWorldSystem>().RemoveCharacterFromList(this);
            _prefabConfig.HealthBar.gameObject.SetActive(false);
            Destroy(gameobject);
        }
        else
        {
            yield return _prefabConfig.HealthBar.SetHealth(_healthStatus.Current.Value);
        }
    }

    public Transform TakeTransform() => transform;

    private void LoadConfig()
    {
        CharacterConfig = new ReactVariable<CharacterConfig>(GameLocator.Get<WorldSaveConfig>().PlayerConfig);
    }

    IAnimatorController IAnimatableCharacter.AnimatorController() => AnimatorController;

    private void OnDestroy()
    {
        GameLocator.Get<GameWorldSystem>()?.RemoveCharacterFromList(this);
        (_stateController as System.IDisposable)?.Dispose();
        (_interactionEnvironment as System.IDisposable)?.Dispose();
        _disposables.Dispose();
    }
}
