using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Boss Chase Direct", menuName = "Enemy Logic/Boss/Boss Chase Direct")]
public class BossChaseSO : EnemyChaseSOBase
{
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float destinationRefreshRate = 0.1f;

    private EnemyNavMeshAgent2D navAgent2D;
    private NavMeshAgent agent;
    private float refreshTimer;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);

        navAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();
        agent = gameObject.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = true;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = movementSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
        }

        Debug.Log($"[BossChaseSO] Initialized for {gameObject.name}. navAgent2D: {(navAgent2D != null ? "OK" : "NULL")}, agent: {(agent != null ? "OK" : "NULL")}");
    }

    public override void DoEnterLogic()
    {
        refreshTimer = 0f;

        enemy.MoveEnemy(Vector2.zero);

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.speed = movementSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
        }
    }

    public override void DoExitLogic()
    {
        navAgent2D?.Stop();
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        if (enemy.PlayerTarget == null)
        {
            Debug.LogWarning("[BossChaseSO] PlayerTarget is null.");
            return;
        }

        if (enemy.IsWithinStrikingDistance)
        {
            navAgent2D?.Stop();
            enemy.MoveEnemy(Vector2.zero);
            enemy.StateMachine.ChangeState(enemy.AttackState);
            return;
        }

        if (!enemy.IsAggroed)
        {
            navAgent2D?.Stop();
            enemy.MoveEnemy(Vector2.zero);
            enemy.InvestigationTargetPosition = enemy.PlayerTarget.position;
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        if (agent == null || !agent.enabled)
        {
            Debug.LogWarning("[BossChaseSO] NavMeshAgent missing or disabled.");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("[BossChaseSO] Agent is not on NavMesh.");
            return;
        }

        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = destinationRefreshRate;
            navAgent2D?.MoveTo(enemy.PlayerTarget.position);
        }

        Vector2 velocity = agent.velocity;
        if (velocity.sqrMagnitude > 0.001f)
            enemy.CheckForLeftOrRightFacing(velocity);
    }

    public override void DoPhysicsLogic()
    {
    }
}