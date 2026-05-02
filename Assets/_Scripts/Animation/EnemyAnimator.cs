using System.Collections;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private static readonly int StateHash = Animator.StringToHash("State");

    [SerializeField] private Animator animator;

    private EnemyAnimState _currentState = (EnemyAnimState)(-1);

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void PlayState(EnemyAnimState state)
    {
        if (animator == null)
            return;

        if (_currentState == state && state != EnemyAnimState.Attack)
            return;

        _currentState = state;
        animator.SetInteger(StateHash, (int)state);
    }

    public void PlayAttack()
    {
        PlayAttack(restartFromBeginning: true);
    }

    public void PlayAttack(bool restartFromBeginning)
    {
        if (animator == null)
            return;

        _currentState = EnemyAnimState.Attack;
        animator.SetInteger(StateHash, (int)EnemyAnimState.Attack);

        if (restartFromBeginning)
        {
            // Force is important for repeated attacks: it restarts the Attack clip every cycle.
            animator.Play("Attack", 0, 0f);
            animator.Update(0f);
        }
    }

    public IEnumerator PlayAttackAndWait(float fallbackDuration)
    {
        PlayAttack(restartFromBeginning: true);
        yield return WaitForAttackAnimation(fallbackDuration);
    }

    public IEnumerator WaitForAttackAnimation(float fallbackDuration)
    {
        float duration = GetAttackClipDuration(fallbackDuration);
        yield return new WaitForSeconds(duration);
    }

    private float GetAttackClipDuration(float fallbackDuration)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return Mathf.Max(0.01f, fallbackDuration);

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        float bestLength = 0f;

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip == null)
                continue;

            if (clip.name.ToLowerInvariant().Contains("attack"))
                bestLength = Mathf.Max(bestLength, clip.length);
        }

        // If Unity does not expose the override clip here, fall back to serialized timing.
        return Mathf.Max(0.01f, bestLength > 0f ? bestLength : fallbackDuration);
    }

    public void PlayDeath()
    {
        PlayState(EnemyAnimState.Death);
    }
}
