using UnityEngine;
using System.Collections;
using UniRx;

public class EnemyCharacter : BaseCharacter, IDamageableTargetable
{
    private BaseCharacterPrefabConfig _prefabConfig;
 
    private IMove _movement;
    private IAnimatorController _animatorController;
    private IStateController _stateController;

    private HealthStatus _healthStatus;

    private CompositeDisposable _disposables = new CompositeDisposable();

    public override void Init()
    {
        _prefabConfig = GetComponent<BaseCharacterPrefabConfig>();
        _healthStatus = new HealthStatus();
        base.Init();
        start();
    }

    public override void Update()
    {
        base.Update();
        _stateController?.Update();
    }

    private void LateUpdate()
    {
        _prefabConfig?.HealthBar.FollowCameraRotate();
    }

    public void TakeDamage(float amount = 0, BaseCharacter damageVisitor = null)
    {
        if (_healthStatus.IsDead.Value) return;
        coroutines.Instance.Execute(Damage(amount, damageVisitor));
    }

    private IEnumerator Damage(float amount, BaseCharacter damageVisitor = null)
    {
        if (_healthStatus.TakeDamage(amount))
        {
            GameLocator.Get<GameWorldSystem>().RemoveCharacterFromList(this);
            _prefabConfig.HealthBar.gameObject.SetActive(false);
            _movement?.Stop();
            _stateController = null;
            Destroy(gameobject); 
        }
        else
        {
            if (!_prefabConfig.HealthBar.gameObject.activeSelf)
                _prefabConfig.HealthBar.gameObject.SetActive(true);

            yield return _prefabConfig.HealthBar.SetHealth(_healthStatus.Current.Value);
        }
    }

    private void start()
    {
        LoadConfig();

        Weapon_react.data = _prefabConfig.Weapon;
        Weapon_react.data.SetHolder(this);

        _prefabConfig.Detector.Init(this);

        _prefabConfig.HealthBar.SetMaxHealth(CharacterConfig.data.Health.data);
        _healthStatus.Current.Value = CharacterConfig.data.Health.data;

        _animatorController = new AnimatorController(
            _prefabConfig.Animator,
            _prefabConfig.AnimEvent,
            AnimatorControllerParameterType.Trigger,
            new AnimatorController.AnimConfig(CharacterConfig.data.CharacterType, Weapon_react)
        );

        _movement = new MovementWithNavMesh(this, _prefabConfig.Agent, CharacterConfig.data.WalkSpeed.data);

        _stateController = new EnemyAIController(
            this,
            CharacterConfig,
            _animatorController,
            _movement,
            Weapon_react,
            _prefabConfig.Detector,
            _prefabConfig
        );

        EventSystem.OnPlayerLanding
            .Subscribe(landingData =>
            {
                if (landingData.Success)
                {
                    GameLocator.Get<GameWorldSystem>().AddCharacterToList(this);
                    _prefabConfig.Agent.enabled = true;
                }
            })
            .AddTo(_disposables);             
    }

    public Transform TakeTransform() => transform;

    private void LoadConfig()
    {
        CharacterConfig = new ReactVariable<CharacterConfig>(GameLocator.Get<WorldSaveConfig>().EnemyConfig);
    }

    private void OnDestroy()
    {
        _disposables.Clear();
        (_stateController as System.IDisposable)?.Dispose();
    }
}
