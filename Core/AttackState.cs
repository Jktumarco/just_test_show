using System.Collections;

public class AttackState : BaseState
{
    private Coroutine coroutine;
    private bool can = false;

    public AttackState(StateMachine stateMachine, StateHelper stateHelper)
        : base(stateMachine, stateHelper) { }

    public override void Enter()
    {
        EventSystem.OnMoveRotateWithJoy.Value = false;
        can = true;
        stateHelper.StopMoving();
        stateHelper.SetAnimation("idle");
        coroutine = Coroutines.Instance.Execute(StateUpdateLoop());
    }

    public override void Update() { }

    private IEnumerator StateUpdateLoop()
    {
        while (can)
        {
            if (!stateHelper.HasBullet())
            {
                stateHelper.ReloadCurrent();
                if (!stateHelper.TryOnChangeWeaponAutomatic())
                {
                    stateHelper.ClearTarget();
                    stateHelper.SetAnimation("idle");
                    yield break;
                }
                else
                {
                    stateHelper.ClearTarget();
                }
            }

            if (!stateHelper.HasValidTarget())
            {
                stateHelper.ClearTarget();
                stateMachine.ChangeState(new SearchState(stateMachine, stateHelper));
                yield break;
            }

            if (!stateHelper.IsTargetLive())
            {
                stateHelper.ClearTarget();
                if (!stateHelper.TryFindTarget())
                {
                    stateMachine.ChangeState(new SearchState(stateMachine, stateHelper));
                    yield break;
                }
            }

            while (!stateHelper.RotateTowardsTarget())
            {
                yield return null;
            }

            if (stateHelper.IsDetectEnemy() && stateHelper.CanAttack())
            {
                stateHelper.AttackTarget("attack");
                yield return new WaitUntil(() => stateHelper.IsAnimationFinished("attack"));
                stateHelper.SetAnimation("idle");
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public override void Exit()
    {
        can = false;
        if (coroutine != null)
            Coroutines.Instance.Stop(coroutine);
        stateHelper.SetAnimation("idle");
        stateHelper.StopMoving();
    }
}
