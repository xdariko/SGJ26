using UnityEngine;
using UnityEngine.InputSystem;

public class Main : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Animator Controllers")]
    [SerializeField] private RuntimeAnimatorController firstMutationController;
    [SerializeField] private RuntimeAnimatorController secondMutationController;
    [SerializeField] private RuntimeAnimatorController thirdMutationController;

    [Header("Passage Blockers")]
    [SerializeField] private Collider2D blockerAfterBoss1;
    [SerializeField] private Collider2D blockerAfterBoss2;

    private bool firstStoryPlayed;
    private bool boss1StoryPlayed;
    private bool boss2StoryPlayed;
    private bool finalStoryPlayed;

    private bool gameEnded;

    private void Awake()
    {
        G.main = this;

        if (playerCombat == null)
        {
            Player player = FindFirstObjectByType<Player>();

            if (player != null)
                playerCombat = player.GetComponent<PlayerCombat>();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        G.IsPaused = false;

        G.storyPanel?.PlaySequence("intro_01");
    }

    private void Update()
    {
        if (gameEnded)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        SetPause(!G.IsPaused);
    }

    private void SetPause(bool paused)
    {
        G.IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (G.ui != null)
            G.ui.SetPausePanel(paused);
    }

    public void ResumeGame()
    {
        if (gameEnded)
            return;

        SetPause(false);
    }

    public void PlayFirstMutationStory()
    {
        if (firstStoryPlayed)
            return;

        firstStoryPlayed = true;

        G.storyPanel?.PlaySequence("metamorphosis_01", () =>
        {
            UnlockFirstAbility();
            ChangePlayerAnimator(firstMutationController);
        });
    }

    public void OnBoss1Killed()
    {
        if (boss1StoryPlayed)
            return;

        boss1StoryPlayed = true;

        G.storyPanel?.PlaySequence("metamorphosis_02", () =>
        {
            UnlockSecondAbility();
            ChangePlayerAnimator(secondMutationController);
            OpenPassage(blockerAfterBoss1);
        });
    }

    public void OnBoss2Killed()
    {
        if (boss2StoryPlayed)
            return;

        boss2StoryPlayed = true;

        G.storyPanel?.PlaySequence("metamorphosis_03", () =>
        {
            UnlockThirdAbility();
            ChangePlayerAnimator(thirdMutationController);
            OpenPassage(blockerAfterBoss2);
        });
    }

    public void OnBoss3Killed()
    {
        if (finalStoryPlayed)
            return;

        finalStoryPlayed = true;

        G.storyPanel?.PlaySequence("final_01", () =>
        {
            EndGame();
        });
    }

    private void UnlockFirstAbility()
    {
        PlayerCombat combat = GetPlayerCombat();

        if (combat == null)
        {
            Debug.LogWarning("Main: PlayerCombat not found, cannot unlock first ability.");
            return;
        }

        combat.UnlockRangedAbility();

        Debug.Log("First ability unlocked: Ranged attack");
    }

    private void UnlockSecondAbility()
    {
        PlayerCombat combat = GetPlayerCombat();

        if (combat == null)
        {
            Debug.LogWarning("Main: PlayerCombat not found, cannot unlock second ability.");
            return;
        }

        combat.UnlockHookAbility();

        Debug.Log("Second ability unlocked: Hook");
    }

    private void UnlockThirdAbility()
    {
        PlayerCombat combat = GetPlayerCombat();

        if (combat == null)
        {
            Debug.LogWarning("Main: PlayerCombat not found, cannot unlock third ability.");
            return;
        }

        combat.UnlockAreaAttackAbility();

        Debug.Log("Third ability unlocked: Area attack");
    }

    private void ChangePlayerAnimator(RuntimeAnimatorController controller)
    {
        if (playerAnimator == null)
        {
            Debug.LogWarning("Player Animator is not assigned.");
            return;
        }

        if (controller == null)
        {
            Debug.LogWarning("New Animator Controller is not assigned.");
            return;
        }

        playerAnimator.runtimeAnimatorController = controller;
    }

    private void OpenPassage(Collider2D blocker)
    {
        if (blocker != null)
            blocker.enabled = false;
    }

    private void EndGame()
    {
        gameEnded = true;

        G.IsPaused = true;
        Time.timeScale = 0f;

        if (G.ui != null)
            G.ui.ShowEndPanel();

        Debug.Log("Game ended");
    }

    private PlayerCombat GetPlayerCombat()
    {
        if (playerCombat != null)
            return playerCombat;

        if (G.player != null)
            playerCombat = G.player.GetComponent<PlayerCombat>();

        if (playerCombat == null)
        {
            Player player = FindFirstObjectByType<Player>();

            if (player != null)
                playerCombat = player.GetComponent<PlayerCombat>();
        }

        return playerCombat;
    }
}