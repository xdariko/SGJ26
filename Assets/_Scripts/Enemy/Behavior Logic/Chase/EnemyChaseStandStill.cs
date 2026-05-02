using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Chase-Stand Still", menuName = "Enemy Logic/Chase Logic/Stand Still")]
public class EnemyChaseStandStill : EnemyChaseSOBase
{
    private EnemyNavMeshAgent2D _navMeshAgent2D;
    private NavMeshAgent _agent;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _navMeshAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        _agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override void DoEnterLogic()
    {
        StopEnemyCompletely();
    }

    public override void DoExitLogic()
    {
        StopEnemyCompletely();
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        StopEnemyCompletely();

        if (playerTransform != null)
        {
            Vector2 directionToPlayer = playerTransform.position - enemy.transform.position;
            enemy.CheckForLeftOrRightFacing(directionToPlayer);
        }

        if (!enemy.IsAggroed)
        {
            if (playerTransform != null)
                enemy.InvestigationTargetPosition = playerTransform.position;

            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        enemy.StateMachine.ChangeState(enemy.AttackState);
    }

    private void StopEnemyCompletely()
    {
        _navMeshAgent2D?.Stop();

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }

        enemy.MoveEnemy(Vector2.zero);
    }
}
