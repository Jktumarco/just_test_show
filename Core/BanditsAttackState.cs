using UnityEngine;
using System.Collections;

namespace BanditsAI
{
    public class BanditsAttackState : BaseState
    {
        private coroutine _attackCoroutine;

        public BanditsAttackState(StateMachine stateMachine, StateHelper stateHelper)
            : base(stateMachine, stateHelper)
        {
        }

        public override void Enter()
        {
            _attackCoroutine = coroutines.Instance.Execute(AttackLoop());
        }

        public override void Update() { }

        public override void Exit()
        {
            if (_attackCoroutine != null)
                coroutines.Instance.Stop(_attackCoroutine);
            stateHelper.StopMoving();
            stateHelper.SetAnimation("idle");
        }

        private IEnumerator AttackLoop()
        {
            while (true)
            {
                while (!stateHelper.RotateTowardsTarget())
                {
                    yield return null;
                }

                if (stateHelper.CanAttack())
                {
                    stateHelper.AttackTarget("attack");
                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
