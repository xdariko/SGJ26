using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Investigate-Basic", menuName = "Enemy Logic/Investigate Logic/Basic")]
public class EnemyInvestigateBasic : EnemyInvestigateSOBase
{
    private enum InvestigatePhase
    {
        MovingToPoint,
        LookingAround,
        MovingAround,
        Completed
    }

    [Header("Movement")]
    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private float _stoppingDistance = 0.35f;
    [SerializeField] private float _destinationReachedExtraDistance = 0.15f;

    [Header("Investigation")]
    [SerializeField] private float _investigationRadius = 2f;
    [SerializeField] private float _lookAroundDuration = 1.5f;
    [SerializeField] private float _timeBetweenSteps = 0.5f;
    [SerializeField] private int _totalSteps = 3;
    [SerializeField] private int _sampleAttempts = 10;

    private InvestigatePhase _currentPhase;
    private EnemyNavMeshAgent2D _navMeshAgent2D;
    private NavMeshAgent _agent;

    private Vector3 _investigationPoint;
    private Vector3 _currentTarget;
    private float _timer;
    private int _stepsCompleted;
    private bool _hasDestination;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);

        _navMeshAgent2D = gameObject.GetComponent<EnemyNavMeshAgent2D>();

        if (_navMeshAgent2D != null)
            _agent = _navMeshAgent2D.Agent;
        else
            _agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        _investigationPoint = GetNearestNavMeshPoint(enemy.InvestigationTargetPosition);
        _currentPhase = InvestigatePhase.MovingToPoint;
        _stepsCompleted = 0;
        _timer = 0f;
        _hasDestination = false;

        SetupAgent(_movementSpeed, _stoppingDistance);
        SetDestination(_investigationPoint);
    }

    public override void DoExitLogic()
    {
        StopAgent();
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        if (enemy.IsAggroed)
        {
            StopAgent();
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (enemy.IsNoiseHeard)
        {
            enemy.IsNoiseHeard = false;

            _investigationPoint = GetNearestNavMeshPoint(enemy.InvestigationTargetPosition);
            _currentPhase = InvestigatePhase.MovingToPoint;
            _stepsCompleted = 0;
            _timer = 0f;

            SetDestination(_investigationPoint);
            return;
        }

        switch (_currentPhase)
        {
            case InvestigatePhase.MovingToPoint:
                HandleMovingToPoint();
                break;

            case InvestigatePhase.LookingAround:
                HandleLookingAround();
                break;

            case InvestigatePhase.MovingAround:
                HandleMovingAround();
                break;

            case InvestigatePhase.Completed:
                StopAgent();
                enemy.StateMachine.ChangeState(enemy.IdleState);
                break;
        }

        FaceByAgentVelocity();
    }

    private void HandleMovingToPoint()
    {
        if (!IsDestinationReached())
            return;

        StopAgent();
        _currentPhase = InvestigatePhase.LookingAround;
        _timer = _lookAroundDuration;
    }

    private void HandleLookingAround()
    {
        _timer -= Time.deltaTime;

        if (_timer > 0f)
            return;

        if (TrySetNewRandomTarget())
        {
            _currentPhase = InvestigatePhase.MovingAround;
        }
        else
        {
            _stepsCompleted++;

            _currentPhase = _stepsCompleted >= _totalSteps
                ? InvestigatePhase.Completed
                : InvestigatePhase.LookingAround;

            _timer = _timeBetweenSteps;
        }
    }

    private void HandleMovingAround()
    {
        if (!IsDestinationReached())
            return;

        StopAgent();
        _stepsCompleted++;

        if (_stepsCompleted >= _totalSteps)
        {
            _currentPhase = InvestigatePhase.Completed;
            return;
        }

        _timer = _timeBetweenSteps;
        _currentPhase = InvestigatePhase.LookingAround;
    }

    private bool TrySetNewRandomTarget()
    {
        for (int i = 0; i < _sampleAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * _investigationRadius;
            Vector3 randomPoint = _investigationPoint + new Vector3(randomCircle.x, randomCircle.y, 0f);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, _investigationRadius, NavMesh.AllAreas))
            {
                _currentTarget = hit.position;

                SetupAgent(_movementSpeed * 0.5f, _stoppingDistance);
                SetDestination(_currentTarget);

                return true;
            }
        }

        return false;
    }

    private Vector3 GetNearestNavMeshPoint(Vector3 point)
    {
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, _investigationRadius + 1f, NavMesh.AllAreas))
            return hit.position;

        return enemy.transform.position;
    }

    private void SetupAgent(float speed, float stoppingDistance)
    {
        if (_agent == null)
            return;

        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _agent.speed = speed;
        _agent.stoppingDistance = stoppingDistance;
        _agent.isStopped = false;
    }

    private void SetDestination(Vector3 destination)
    {
        _hasDestination = true;

        if (_navMeshAgent2D != null)
        {
            _navMeshAgent2D.MoveTo(destination);
            return;
        }

        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            return;

        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    private bool IsDestinationReached()
    {
        if (!_hasDestination)
            return true;

        if (_navMeshAgent2D != null)
            return _navMeshAgent2D.HasReachedDestination(_destinationReachedExtraDistance);

        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            return true;

        if (_agent.pathPending)
            return false;

        return _agent.remainingDistance <= _agent.stoppingDistance + _destinationReachedExtraDistance;
    }

    private void StopAgent()
    {
        _hasDestination = false;

        if (_navMeshAgent2D != null)
        {
            _navMeshAgent2D.Stop();
            return;
        }

        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }

        enemy.MoveEnemy(Vector2.zero);
    }

    private void FaceByAgentVelocity()
    {
        if (_agent == null)
            return;

        Vector2 velocity = _agent.velocity;

        if (velocity.sqrMagnitude > 0.001f)
            enemy.CheckForLeftOrRightFacing(velocity);
    }

    public override void ResetValues()
    {
        base.ResetValues();

        _currentPhase = InvestigatePhase.MovingToPoint;
        _stepsCompleted = 0;
        _timer = 0f;
        _hasDestination = false;
    }
}