using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyAnimator : MonoBehaviour
{
    private static readonly int StateHash = Animator.StringToHash("State");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    private static readonly int HitHash = Animator.StringToHash("Hit");

    [SerializeField] private Animator _animator;

    private EnemyAnimState _currentState;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    public void PlayState(EnemyAnimState state)
    {
        if (_animator == null)
            return;

        if (_currentState == state)
            return;

        _currentState = state;
        _animator.SetInteger(StateHash, (int)state);
    }

    public void PlayAttack()
    {
        if (_animator == null)
            return;

        _currentState = EnemyAnimState.Attack;
        _animator.SetInteger(StateHash, (int)EnemyAnimState.Attack);
        _animator.SetTrigger(AttackHash);
    }

    public IEnumerator PlayAttackAndWait(float fallbackDuration)
    {
        PlayAttack();
        yield return WaitForAttackAnimation(fallbackDuration);
    }

    public IEnumerator WaitForAttackAnimation(float fallbackDuration)
    {
        if (_animator == null)
        {
            yield return new WaitForSeconds(fallbackDuration);
            yield break;
        }

        // Give the Animator one frame to consume State/Attack and start the transition.
        yield return null;

        yield return new WaitForSeconds(GetAttackAnimationDuration(fallbackDuration));
    }

    public float GetAttackAnimationDuration(float fallbackDuration)
    {
        if (_animator == null)
            return fallbackDuration;

        RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
        if (controller != null)
        {
            foreach (AnimationClip clip in controller.animationClips)
            {
                if (clip != null && clip.name.ToLowerInvariant().Contains("attack"))
                    return GetScaledDuration(clip.length, fallbackDuration);
            }
        }

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return GetScaledDuration(stateInfo.length, fallbackDuration);
    }

    private float GetScaledDuration(float duration, float fallbackDuration)
    {
        if (duration <= 0f)
            duration = fallbackDuration;

        float speed = _animator != null ? Mathf.Abs(_animator.speed) : 1f;
        if (speed <= 0f)
            speed = 1f;

        return duration / speed;
    }

    public void PlayDeath()
    {
        if (_animator == null)
            return;

        _currentState = EnemyAnimState.Death;
        _animator.SetInteger(StateHash, (int)EnemyAnimState.Death);
        _animator.SetTrigger(DeathHash);
    }

    public void PlayHit()
    {
        if (_animator == null)
            return;

        _animator.SetTrigger(HitHash);
    }
}
