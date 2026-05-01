using UnityEngine;

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

    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private float _investigationRadius = 2f;
    [SerializeField] private float _lookAroundDuration = 1.5f;
    [SerializeField] private float _timeBetweenSteps = 0.5f;
    [SerializeField] private int _totalSteps = 3;

    private InvestigatePhase _currentPhase;
    private Vector3 _investigationPoint;
    private Vector3 _currentTarget;
    private float _timer;
    private int _stepsCompleted;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        _investigationPoint = enemy.InvestigationTargetPosition;
        _currentPhase = InvestigatePhase.MovingToPoint;
        _stepsCompleted = 0;
        _timer = 0f;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        enemy.MoveEnemy(Vector2.zero);
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

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
                enemy.StateMachine.ChangeState(enemy.IdleState);
                break;
        }
    }

    private void HandleMovingToPoint()
    {
        Vector2 moveDirection = (_investigationPoint - transform.position).normalized;
        enemy.MoveEnemy(moveDirection * _movementSpeed);
        enemy.CheckForLeftOrRightFacing(moveDirection);

        if (Vector2.Distance(transform.position, _investigationPoint) < 0.5f)
        {
            enemy.MoveEnemy(Vector2.zero);
            _currentPhase = InvestigatePhase.LookingAround;
            _timer = _lookAroundDuration;
        }
    }

    private void HandleLookingAround()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            _currentPhase = InvestigatePhase.MovingAround;
            SetNewRandomTarget();
        }
    }

    private void HandleMovingAround()
    {
        Vector2 moveDirection = (_currentTarget - transform.position).normalized;
        enemy.MoveEnemy(moveDirection * (_movementSpeed * 0.5f));
        enemy.CheckForLeftOrRightFacing(moveDirection);

        if (Vector2.Distance(transform.position, _currentTarget) < 0.3f)
        {
            enemy.MoveEnemy(Vector2.zero);
            _stepsCompleted++;

            if (_stepsCompleted >= _totalSteps)
            {
                _currentPhase = InvestigatePhase.Completed;
            }
            else
            {
                _timer = _timeBetweenSteps;
                _currentPhase = InvestigatePhase.LookingAround;
            }
        }
    }

    private void SetNewRandomTarget()
    {
        _currentTarget = _investigationPoint + (Vector3)Random.insideUnitCircle * _investigationRadius;
    }

    public override void ResetValues()
    {
        base.ResetValues();
        _currentPhase = InvestigatePhase.MovingToPoint;
        _stepsCompleted = 0;
        _timer = 0f;
    }
}