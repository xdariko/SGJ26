using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] protected BossAttackControllerSO attackController;
    [SerializeField] public float postSpecialDelay = 0.5f;
    [SerializeField] protected float vulnerableDuration = 2f;

    [Header("Vulnerability (Tank/Agile)")]
    [SerializeField] protected bool useRangedVulnerability = false;
    [SerializeField] protected bool useHookVulnerability = false;

    public bool IsPerformingSpecial { get; set; }
    public bool VulnerableToRanged { get; private set; }
    public bool VulnerableToHook { get; private set; }
    public bool UsesRangedVulnerability => useRangedVulnerability;
    public bool UsesHookVulnerability => useHookVulnerability;
    public List<GameObject> ActiveMinions { get; } = new List<GameObject>();
    public Animator Animator { get; set; }
    public BossAttackControllerSO AttackController => attackController;

    protected override void Awake()
    {
        base.Awake();
        Animator = GetComponent<Animator>();
        Debug.Log($"[BossEnemy] Awake: Animator present: {Animator != null}");
    }

    protected override void Start()
    {
        base.Start();
        Debug.Log($"[BossEnemy] Start: attackController={(attackController != null ? attackController.name : "NULL")}");
        
        // Diagnostic checks
        Debug.Log($"[BossEnemy] Components check:");
        Debug.Log($"  - NavMeshAgent: {(GetComponent<UnityEngine.AI.NavMeshAgent>() != null ? "OK" : "MISSING")}");
        Debug.Log($"  - EnemyNavMeshAgent2D: {(GetComponent<EnemyNavMeshAgent2D>() != null ? "OK" : "MISSING")}");
        Debug.Log($"  - Rigidbody2D: {(GetComponent<Rigidbody2D>() != null ? "OK" : "MISSING")}");
        Debug.Log($"  - Animator: {(GetComponent<Animator>() != null ? "OK" : "MISSING")}");
        Debug.Log($"  - EnemyHealth: {(GetComponent<EnemyHealth>() != null ? "OK" : "MISSING")}");
        Debug.Log($"  - EnemyAggroCheck: {(GetComponentInChildren<EnemyAggroCheck>(true) != null ? "OK" : "MISSING")}");
        Debug.Log($"  - EnemyStrikingDistanceCheck: {(GetComponentInChildren<EnemyStrikingDistanceCheck>(true) != null ? "OK" : "MISSING")}");
        
        if (attackController != null)
        {
            attackController.Initialize(this);
            Debug.Log($"[BossEnemy] AttackController initialized. Abilities count: {attackController.abilities?.Count ?? 0}");
            if (attackController.abilities != null)
            {
                for (int i = 0; i < attackController.abilities.Count; i++)
                {
                    var entry = attackController.abilities[i];
                    Debug.Log($"[BossEnemy] Ability {i}: Type={entry.Type}, Cooldown={entry.Cooldown}, Range={entry.MinRange}-{entry.MaxRange}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[BossEnemy] AttackController is NULL! Please assign BossAttackControllerSO.", this);
        }

        Debug.Log($"[BossEnemy] AttackBase check: EnemyAttackBaseInstance type: {EnemyAttackBaseInstance?.GetType().Name ?? "NULL"}");
        Debug.Log($"[BossEnemy] AttackBase is BossAttackSO: {EnemyAttackBaseInstance is BossAttackSO}");
        
        VulnerableToRanged = false;
        VulnerableToHook = false;
    }

    private new void Update()
    {
        base.Update();
        if (!IsPerformingSpecial)
        {
            attackController?.UpdateAbilities(this, StateMachine);
        }
        Debug.Log($"[BossEnemy] Update: IsAggroed={IsAggroed}, State={StateMachine?.CurrentEnemyState?.GetType().Name}, PlayerTargetExists={PlayerTarget != null}, IsPerformingSpecial={IsPerformingSpecial}");
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public virtual void StartSpecialAbility(int abilityIndex)
    {
        if (IsPerformingSpecial) return;
        if (attackController == null) return;

        Debug.Log($"[BossEnemy] StartSpecialAbility: abilityIndex={abilityIndex}, type={attackController.GetSpecialAnimation(abilityIndex)}");
        IsPerformingSpecial = true;

        EnemyState specialState = attackController.CreateSpecialState(abilityIndex, this);
        if (specialState != null)
        {
            StateMachine.ChangeState(specialState);
        }
        else
        {
            Debug.LogWarning($"[BossEnemy] CreateSpecialState returned null for abilityIndex {abilityIndex}");
        }

        string animTrigger = attackController.GetSpecialAnimation(abilityIndex);
        if (!string.IsNullOrEmpty(animTrigger) && Animator != null)
        {
            Animator.SetTrigger(animTrigger);
        }
    }

    public virtual void SetVulnerableToRanged(bool vulnerable)
    {
        SetVulnerableToRanged(vulnerable, vulnerableDuration);
    }

    public virtual void SetVulnerableToRanged(bool vulnerable, float duration)
    {
        VulnerableToRanged = vulnerable;
        if (vulnerable && duration > 0f)
        {
            CancelInvoke(nameof(ResetVulnerableRanged));
            Invoke(nameof(ResetVulnerableRanged), duration);
        }
    }

    private void ResetVulnerableRanged() => VulnerableToRanged = false;

    public virtual void SetVulnerableToHook(bool vulnerable)
    {
        SetVulnerableToHook(vulnerable, vulnerableDuration);
    }

    public virtual void SetVulnerableToHook(bool vulnerable, float duration)
    {
        VulnerableToHook = vulnerable;
        if (vulnerable && duration > 0f)
        {
            CancelInvoke(nameof(ResetVulnerableHook));
            Invoke(nameof(ResetVulnerableHook), duration);
        }
    }

    private void ResetVulnerableHook() => VulnerableToHook = false;

    public virtual void RegisterMinion(GameObject minion)
    {
        if (!ActiveMinions.Contains(minion))
        {
            ActiveMinions.Add(minion);
            EnemyHealth health = minion.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.OnDeath += () => UnregisterMinion(minion);
            }
        }
    }

    public virtual void UnregisterMinion(GameObject minion) => ActiveMinions.Remove(minion);

    public virtual void ClearMinions()
    {
        foreach (GameObject minion in new List<GameObject>(ActiveMinions))
        {
            if (minion != null) Destroy(minion);
        }
        ActiveMinions.Clear();
    }

    private void OnDestroy() => ClearMinions();
}
