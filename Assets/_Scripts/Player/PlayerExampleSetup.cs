using UnityEngine;

public class PlayerExampleSetup : MonoBehaviour
{
    [Header("Ability References")]
    public MeleeAbility meleeAbility;
    public RangedAbility rangedAbility;
    public HookAbility hookAbility;
    public AreaAttackAbility areaAttackAbility;

    [Header("Auto Setup")]
    public bool autoSetupPlayer = true;
    public bool unlockAllAbilitiesByDefault = false;

    private void Awake()
    {
        if (autoSetupPlayer)
        {
            SetupPlayerComponents();
        }
    }

    private void SetupPlayerComponents()
    {
        Player player = GetComponent<Player>();
        PlayerController playerController = GetComponent<PlayerController>();
        PlayerCombat combat = GetComponent<PlayerCombat>();
        AbilityManager abilityManager = GetComponent<AbilityManager>();

        if (player == null)
            player = gameObject.AddComponent<Player>();

        if (playerController == null)
            playerController = gameObject.AddComponent<PlayerController>();

        if (combat == null)
            combat = gameObject.AddComponent<PlayerCombat>();

        if (abilityManager == null)
            abilityManager = gameObject.AddComponent<AbilityManager>();

        // Важно: способности надо назначать всегда,
        // даже если они пока закрыты.
        if (combat != null)
        {
            combat.SetMeleeAbility(meleeAbility);
            combat.SetRangedAbility(rangedAbility);
            combat.SetHookAbility(hookAbility);
            combat.SetAreaAttackAbility(areaAttackAbility);
        }

        if (abilityManager != null)
        {
            abilityManager.SetPlayerCombat(combat);

            SetupMeleeAbility(abilityManager);
            SetupRangedAbility(abilityManager, combat);
            SetupHookAbility(abilityManager, combat);
            SetupAreaAttackAbility(abilityManager, combat);
        }

        Debug.Log("PlayerExampleSetup: abilities initialized.");
    }

    private void SetupMeleeAbility(AbilityManager abilityManager)
    {
        if (meleeAbility == null)
            return;

        abilityManager.AddAbility("Melee", meleeAbility);
        meleeAbility.Initialize();

        // Обычная атака открыта всегда.
        meleeAbility.Unlock();
    }

    private void SetupRangedAbility(AbilityManager abilityManager, PlayerCombat combat)
    {
        if (rangedAbility == null)
            return;

        abilityManager.AddAbility("Ranged", rangedAbility);
        rangedAbility.Initialize();

        if (unlockAllAbilitiesByDefault)
        {
            rangedAbility.Unlock();

            if (combat != null)
                combat.UnlockRangedAbility();
        }
        else
        {
            rangedAbility.Lock();
        }
    }

    private void SetupHookAbility(AbilityManager abilityManager, PlayerCombat combat)
    {
        if (hookAbility == null)
            return;

        abilityManager.AddAbility("Hook", hookAbility);
        hookAbility.Initialize();

        if (unlockAllAbilitiesByDefault)
        {
            hookAbility.Unlock();

            if (combat != null)
                combat.UnlockHookAbility();
        }
        else
        {
            hookAbility.Lock();
        }
    }

    private void SetupAreaAttackAbility(AbilityManager abilityManager, PlayerCombat combat)
    {
        if (areaAttackAbility == null)
            return;

        abilityManager.AddAbility("AreaAttack", areaAttackAbility);
        areaAttackAbility.Initialize();

        if (unlockAllAbilitiesByDefault)
        {
            areaAttackAbility.Unlock();

            if (combat != null)
                combat.UnlockAreaAttackAbility();
        }
        else
        {
            areaAttackAbility.Lock();
        }
    }

    [ContextMenu("Setup Player Components")]
    private void SetupPlayerComponentsEditor()
    {
        SetupPlayerComponents();
    }
}