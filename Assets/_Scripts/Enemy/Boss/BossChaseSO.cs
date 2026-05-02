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
            agent.speed = movementSpeed;
            agent.stoppingDistance = stoppingDistance;
        }
    }

    public override void DoEnterLogic()
    {
        refreshTimer = 0f;
        enemy.MoveEnemy(Vector2.zero);

        if (agent != null)
        {
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
        // Do not call base.DoFrameUpdateLogic to avoid auto state changes
        if (enemy.PlayerTarget == null) return; // Use enemy.PlayerTarget if available

        // If not aggroed, go to investigate
        if (!enemy.IsAggroed)
        {
            navAgent2D?.Stop();
            enemy.MoveEnemy(Vector2.zero);
            enemy.InvestigationTargetPosition = enemy.PlayerTarget.position;
            enemy.StateMachine.ChangeState(enemy.InvestigateState);
            return;
        }

        // Movement handled in Physics or here? Use NavMeshAgent
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = destinationRefreshRate;
                navAgent2D?.MoveTo(enemy.PlayerTarget.position);
            }
        }

        // Face player based on movement direction
        if (agent != null)
        {
            Vector2 velocity = agent.velocity;
            if (velocity.sqrMagnitude > 0.001f)
                enemy.CheckForLeftOrRightFacing(velocity);
        }
    }
}
