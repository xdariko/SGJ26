using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BossAttack", menuName = "Enemy Logic/Boss/Boss Attack")]
public class BossAttackSO : EnemyAttackSOBase
{
    private BossEnemy _boss;
    private Enemy _enemy;
    private Coroutine _attackRoutine;
    private int _currentAttackIndex;
    private BossAttackControllerSO _controller;

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
        _enemy = enemy;
        _boss = enemy as BossEnemy;
    }

    public override void DoEnterLogic()
    {
        Debug.LogWarning($"[BossAttackSO] DoEnterLogic called on {_enemy?.gameObject.name}");
        
        if (_boss == null)
        {
            Debug.LogError("[BossAttackSO] Boss is NULL! Enemy component reference lost.");
            return;
        }

        // Stop movement
        var nav = _boss.GetComponent<EnemyNavMeshAgent2D>();
        Debug.Log($"[BossAttackSO] NavMeshAgent2D: {(nav != null ? "OK" : "NULL")}");
        nav?.Stop();
        _boss.MoveEnemy(Vector2.zero);

        // Set animation state
        var animator = _boss.EnemyAnimator;
        Debug.Log($"[BossAttackSO] EnemyAnimator: {(animator != null ? "OK" : "NULL")}");
        animator?.PlayState(EnemyAnimState.Attack);

        _controller = _boss.AttackController;
        if (_controller == null)
        {
            Debug.LogError("[BossAttackSO] AttackController is NULL! Assign it in BossEnemy inspector.");
            return;
        }

        _currentAttackIndex = _controller.GetCurrentAttackIndex();
        Debug.Log($"[BossAttackSO] CurrentAttackIndex: {_currentAttackIndex}");
        
        if (_currentAttackIndex < 0)
        {
            Debug.LogError("[BossAttackSO] Invalid attack index! Check AttackController abilities list.");
            return;
        }

        _attackRoutine = _boss.StartCoroutine(ExecuteAttackRoutine());
        Debug.Log($"[BossAttackSO] Attack routine started for ability index {_currentAttackIndex}");
    }

    public override void DoExitLogic()
    {
        if (_attackRoutine != null)
        {
            _boss.StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
        base.DoExitLogic();
    }

    private IEnumerator ExecuteAttackRoutine()
    {
        BossAttackControllerSO.BossAbilityEntry entry = _controller.GetCurrentAttackEntry();
        if (entry == null) yield break;

        Debug.Log($"[BossAttackSO] Executing attack {_currentAttackIndex}: {entry.Type}");

        switch (entry.Type)
        {
            case BossAttackControllerSO.AbilityType.MeleeSlam:
                yield return ExecuteMeleeSlam(entry.meleeSlam);
                break;
            case BossAttackControllerSO.AbilityType.Ranged:
                yield return ExecuteRanged(entry.ranged);
                break;
            case BossAttackControllerSO.AbilityType.QuickShot:
                yield return ExecuteQuickShot(entry.quickShot);
                break;
            default:
                Debug.LogWarning($"[BossAttackSO] Unsupported attack type: {entry.Type}");
                yield break;
        }

        // Return to chase after attack
        _enemy.StateMachine.ChangeState(_boss.ChaseState);
    }

    private IEnumerator ExecuteMeleeSlam(BossAttackControllerSO.MeleeSlamParams p)
    {
        if (_boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            _boss.Animator.SetTrigger(p.animationTrigger);

        yield return new WaitForSeconds(p.windup);

        Collider2D[] hit = Physics2D.OverlapCircleAll((Vector2)_boss.transform.position, p.radius);
        Debug.Log($"[BossAttackSO] MeleeSlam hit {hit.Length} colliders at radius {p.radius}");
        foreach (Collider2D col in hit)
        {
            if (col.CompareTag("Player"))
            {
                IDamageable d = col.GetComponent<IDamageable>();
                if (d != null)
                {
                    Debug.Log($"[BossAttackSO] >>> DAMAGE PLAYER {col.gameObject.name} for {p.damage}");
                    d.TakeDamage(p.damage);
                }
            }
        }

        yield return new WaitForSeconds(p.attackDuration);
    }

    private IEnumerator ExecuteRanged(BossAttackControllerSO.RangedParams p)
    {
        if (_boss.PlayerTarget == null) yield break;

        if (_boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            _boss.Animator.SetTrigger(p.animationTrigger);

        int burstCount = p.burstCount;
        float interval = 1f / p.fireRate;

        for (int i = 0; i < burstCount; i++)
        {
            if (_boss.PlayerTarget == null) yield break;
            FireProjectile(p);
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator ExecuteQuickShot(BossAttackControllerSO.QuickShotParams p)
    {
        if (_boss.PlayerTarget == null) yield break;

        if (_boss.Animator != null && !string.IsNullOrEmpty(p.animationTrigger))
            _boss.Animator.SetTrigger(p.animationTrigger);

        Vector2 baseDir = ((Vector2)_boss.PlayerTarget.position - (Vector2)_boss.transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float halfSpread = p.spreadAngle / 2f;

        for (int i = 0; i < p.volleySize; i++)
        {
            if (_boss.PlayerTarget == null) yield break;
            float angle = baseAngle + (i == 0 ? -halfSpread : (i == 1 ? 0f : halfSpread));
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            if (p.projectilePrefab != null)
            {
                GameObject proj = Object.Instantiate(p.projectilePrefab, _boss.transform.position, Quaternion.identity);
                EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
                if (ep == null) ep = proj.AddComponent<EnemyProjectile>();
                ep.Initialize(dir, p.projectileSpeed, p.damage, p.projectileLifetime, _boss.gameObject);
            }

            yield return new WaitForSeconds(p.volleyDelay);
        }
    }

    private void FireProjectile(BossAttackControllerSO.RangedParams p)
    {
        if (p.projectilePrefab == null) return;
        if (_boss.PlayerTarget == null) return;

        Vector2 dir = ((Vector2)_boss.PlayerTarget.position - (Vector2)_boss.transform.position).normalized;
        GameObject proj = Object.Instantiate(p.projectilePrefab, _boss.transform.position, Quaternion.identity);
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep == null) ep = proj.AddComponent<EnemyProjectile>();
        ep.Initialize(dir, p.projectileSpeed, p.damage, p.projectileLifetime, _boss.gameObject);
    }
}
