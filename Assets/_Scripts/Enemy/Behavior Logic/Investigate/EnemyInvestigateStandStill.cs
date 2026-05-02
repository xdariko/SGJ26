using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Investigate-Stand Still", menuName = "Enemy Logic/Investigate Logic/Stand Still")]
public class EnemyInvestigateStandStill : EnemyInvestigateSOBase
{
    [SerializeField] private float _waitBeforeIdle = 1f;

    private NavMeshAgent _agent;
    private float _timer;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override void DoEnterLogic()
    {
        _timer = _waitBeforeIdle;
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

        StopMovement();

        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            enemy.IsNoiseHeard = false;
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
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