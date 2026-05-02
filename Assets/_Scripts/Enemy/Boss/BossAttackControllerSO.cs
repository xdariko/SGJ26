using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossAttackController", menuName = "Enemy Logic/Boss/Boss Attack Controller")]
public class BossAttackControllerSO : ScriptableObject
{
    public enum AbilityType
    {
        MeleeSlam,
        Ranged,
        QuickShot,
        Charge,
        Dash,
        Summon
    }

    [Header("Global Settings")]
    public float postSpecialDelay = 0.5f;

    [System.Serializable]
    public class BossAbilityEntry
    {
        public AbilityType Type;
        public float Cooldown = 5f;
        public float ChanceWeight = 1f;
        public bool RequireLineOfSight = false;
        public float MinRange = 0f;
        public float MaxRange = 20f;

        // Normal attack parameters
        public MeleeSlamParams meleeSlam;
        public RangedParams ranged;
        public QuickShotParams quickShot;

        // Special ability parameters
        public ChargeParams charge;
        public DashParams dash;
        public SummonParams summon;

        [HideInInspector] public float currentCooldown;
    }

    [System.Serializable]
    public class MeleeSlamParams
    {
        public float damage = 20f;
        public float radius = 1.5f;
        public float windup = 0.3f;
        public float attackDuration = 0.5f;
        public string animationTrigger = "MeleeSlam";
    }

    [System.Serializable]
    public class RangedParams
    {
        public float damage = 10f;
        public float projectileSpeed = 8f;
        public float projectileLifetime = 3f;
        public GameObject projectilePrefab;
        public float fireRate = 1f; // shots per second
        public int burstCount = 1;
        public string animationTrigger = "RangedAttack";
    }

    [System.Serializable]
    public class QuickShotParams
    {
        public int volleySize = 3;
        public float spreadAngle = 15f; // total cone angle
        public float damage = 8f;
        public float projectileSpeed = 10f;
        public float projectileLifetime = 2f;
        public GameObject projectilePrefab;
        public float volleyDelay = 0.1f; // delay between shots
        public string animationTrigger = "QuickShot";
    }

    [System.Serializable]
    public class ChargeParams
    {
        public float chargeSpeed = 12f;
        public float chargeDuration = 0.8f;
        public float chargeDamage = 25f;
        public float aoeRadius = 1.2f;
        public float windup = 0.2f;
        public float vulnerableDuration = 2f;
        public string animationTrigger = "Charge";
    }

    [System.Serializable]
    public class DashParams
    {
        public float dashSpeed = 15f;
        public float dashDuration = 0.4f;
        public bool dashAwayFromPlayer = true; // true = away, false = toward/perp
        public float vulnerableDuration = 2f;
        public string animationTrigger = "Dash";
    }

    [System.Serializable]
    public class SummonParams
    {
        public GameObject[] minionPrefabs;
        public int waveSize = 3;
        public float summonDuration = 5f;
        public bool waitForMinionsDeath = false;
        public string animationTrigger = "Summon";
    }

    [Header("Abilities (Ordered by Priority)")]
    public List<BossAbilityEntry> abilities = new List<BossAbilityEntry>();

    private Dictionary<int, float> cooldownTimers = new Dictionary<int, float>();
    private BossEnemy boss;

    public virtual void Initialize(BossEnemy boss)
    {
        this.boss = boss;
        cooldownTimers.Clear();
        for (int i = 0; i < abilities.Count; i++)
        {
            cooldownTimers[i] = 0f;
        }
    }

    public virtual void UpdateAbilities(BossEnemy boss, EnemyStateMachine stateMachine)
    {
        if (boss == null) return;

        // Update cooldowns always
        for (int i = 0; i < abilities.Count; i++)
        {
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;
            }
        }

        // If boss is performing a special ability, skip selecting new abilities
        if (boss.IsPerformingSpecial) return;

        // Try to find and execute an ability
        for (int i = 0; i < abilities.Count; i++)
        {
            BossAbilityEntry entry = abilities[i];
            if (cooldownTimers[i] > 0f) continue;

            if (!CanUseAbility(i, boss)) continue;

            bool isSpecial = IsSpecialAbility(entry.Type);

            if (isSpecial)
            {
                cooldownTimers[i] = entry.Cooldown;
                boss.StartSpecialAbility(i);
            }
            else
            {
                // Normal attack - ensure we set index before state change
                if (stateMachine.CurrentEnemyState is not EnemyAttackState)
                {
                    SetCurrentAttackIndex(i);
                    cooldownTimers[i] = entry.Cooldown;
                    stateMachine.ChangeState(boss.AttackState);
                }
            }

            break; // Only one ability per check cycle
        }
    }

    protected virtual bool CanUseAbility(int index, BossEnemy boss)
    {
        BossAbilityEntry entry = abilities[index];
        if (!boss.IsAggroed) return false;

        Transform player = boss.PlayerTarget;
        if (player == null) return false;

        // Check distance to player
        float distanceToPlayer = Vector3.Distance(boss.transform.position, player.position);
        if (distanceToPlayer < entry.MinRange || distanceToPlayer > entry.MaxRange)
            return false;

        // Line of sight check
        if (entry.RequireLineOfSight)
        {
            Vector2 dir = (player.position - boss.transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(boss.transform.position, dir, distanceToPlayer);
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
                return false;
        }

        return true;
    }

    public virtual float GetSpecialDuration(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Count) return 0f;
        BossAbilityEntry entry = abilities[abilityIndex];
        AbilityType type = entry.Type;

        switch (type)
        {
            case AbilityType.Charge:
                return entry.charge.windup + entry.charge.chargeDuration;
            case AbilityType.Dash:
                return entry.dash.dashDuration;
            case AbilityType.Summon:
                return entry.summon.summonDuration;
            default:
                return 0f;
        }
    }

    public virtual EnemyState CreateSpecialState(int abilityIndex, BossEnemy boss)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Count) return null;
        BossAbilityEntry entry = abilities[abilityIndex];
        EnemyStateMachine sm = boss.StateMachine;

        switch (entry.Type)
        {
            case AbilityType.Charge:
                return new BossChargeState(boss, sm, entry.charge);
            case AbilityType.Dash:
                return new BossDashState(boss, sm, entry.dash);
            case AbilityType.Summon:
                return new BossSummonState(boss, sm, entry.summon);
            default:
                return null;
        }
    }

    public virtual string GetSpecialAnimation(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Count) return null;
        BossAbilityEntry entry = abilities[abilityIndex];
        switch (entry.Type)
        {
            case AbilityType.Charge: return entry.charge.animationTrigger;
            case AbilityType.Dash: return entry.dash.animationTrigger;
            case AbilityType.Summon: return entry.summon.animationTrigger;
            case AbilityType.MeleeSlam: return entry.meleeSlam.animationTrigger;
            case AbilityType.Ranged: return entry.ranged.animationTrigger;
            case AbilityType.QuickShot: return entry.quickShot.animationTrigger;
            default: return null;
        }
    }

    // For normal attacks
    private int currentAttackIndex = -1;
    public void SetCurrentAttackIndex(int index) => currentAttackIndex = index;
    public int GetCurrentAttackIndex() => currentAttackIndex;
    public BossAbilityEntry GetCurrentAttackEntry()
    {
        if (currentAttackIndex < 0 || currentAttackIndex >= abilities.Count) return null;
        return abilities[currentAttackIndex];
    }

    public MeleeSlamParams GetMeleeSlamParams(int index) => abilities[index].meleeSlam;
    public RangedParams GetRangedParams(int index) => abilities[index].ranged;
    public QuickShotParams GetQuickShotParams(int index) => abilities[index].quickShot;

    protected virtual bool IsSpecialAbility(AbilityType type)
    {
        return type == AbilityType.Charge || type == AbilityType.Dash || type == AbilityType.Summon;
    }
}
