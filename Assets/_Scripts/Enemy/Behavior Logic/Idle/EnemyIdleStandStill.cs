using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Idle-Stand Still", menuName = "Enemy Logic/Idle Logic/Stand Still")]
public class EnemyIdleStandStill : EnemyIdleSOBase
{
    private NavMeshAgent _agent;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override void DoEnterLogic()
    {
        StopMovement();
    }

    public override void DoExitLogic()
    {
        StopMovement();
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        if (enemy.IsAggroed)
        {
            StopMovement();
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (enemy.IsNoiseHeard)
        {
            enemy.IsNoiseHeard = false;
            StopMovement();
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        StopMovement();
    }

    private void StopMovement()
    {
        enemy.MoveEnemy(Vector2.zero);

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }
    }
}